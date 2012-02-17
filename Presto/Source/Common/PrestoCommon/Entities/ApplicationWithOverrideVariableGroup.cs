using System;
using System.Linq;
using Newtonsoft.Json;
using PrestoCommon.Enums;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application can be installed multiple times on any given app server. Using the canonical example, each
    /// instance needs to be the same as the others except for installation path, and one or more custom variables.
    /// </summary>
    public class ApplicationWithOverrideVariableGroup : EntityBase
    {
        private bool _enabled;
        private Application _application;
        private CustomVariableGroup _customVariableGroup;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationWithOverrideVariableGroup"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled
        {
            get { return this._enabled; }

            set
            {
                this._enabled = value;
                NotifyPropertyChanged(() => this.Enabled);
            }
        }

        /// <summary>
        /// Gets or sets the application id.
        /// </summary>
        /// <value>
        /// The application id.
        /// </value>
        public string ApplicationId { get; set; }  // For RavenDB, grrrr...

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public Application Application
        {
            get { return this._application; }

            set
            {
                this._application = value;
                if (this._application != null) { this.ApplicationId = this._application.Id; }
                NotifyPropertyChanged(() => this.Application);
            }
        }

        /// <summary>
        /// Gets or sets the custom variable group id.
        /// </summary>
        /// <value>
        /// The custom variable group id.
        /// </value>
        public string CustomVariableGroupId { get; set; }  // For RavenDB, grrrr...

        /// <summary>
        /// Gets or sets the custom variable group.
        /// </summary>
        /// <value>
        /// The custom variable group.
        /// </value>
        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public CustomVariableGroup CustomVariableGroup
        {
            get { return this._customVariableGroup; }

            set
            {
                this._customVariableGroup = value;
                if (this._customVariableGroup != null) { this.CustomVariableGroupId = this._customVariableGroup.Id; }
                NotifyPropertyChanged(() => this.CustomVariableGroup);
            }
        }

        /// <summary>
        /// Installs this instance.
        /// </summary>
        public InstallationResult Install(ApplicationServer applicationServer)
        {
            bool atLeastOneTaskFailed = false;
            int numberOfSuccessfulTasks = 0;

            try
            {
                // Note: We do a ToList() here because we get a "collection was modified" exception otherwise. The reason we
                //       get the exception is because, somewhere else in this processing, we make this call:
                //       CustomVariableGroupLogic.Get(application.Name)
                //       That method does a refresh on the CustomVariableGroup, which contains an app, which contains the tasks.
                //       Good times.
                foreach (TaskBase taskBase in this.Application.Tasks.ToList().OrderBy(task => task.Sequence))
                {
                    taskBase.Execute(applicationServer, this);

                    if (taskBase.TaskSucceeded == true) { numberOfSuccessfulTasks++; }

                    if (taskBase.TaskSucceeded == false)
                    {
                        atLeastOneTaskFailed = true;
                        if (taskBase.FailureCausesAllStop == 1) { break; }  // No more processing.
                    }
                }

                if (numberOfSuccessfulTasks < 1) { return InstallationResult.Failure; }

                if (atLeastOneTaskFailed) { return InstallationResult.PartialSuccess; }

                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                return InstallationResult.Failure;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string appAndVersion = string.Empty;

            if (this.Application != null) { appAndVersion = this.Application.Name + " " + this.Application.Version; }            

            string groupNameSuffix = string.Empty;

            if (this.CustomVariableGroup != null) { groupNameSuffix = " with " + this.CustomVariableGroup.Name; }

            return appAndVersion + groupNameSuffix;
        }
    }
}
