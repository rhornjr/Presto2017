using System;
using System.Linq;
using System.Runtime.Serialization;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Enums;
using PrestoCommon.Misc;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application can be installed multiple times on any given app server. Using the canonical example, each
    /// instance needs to be the same as the others except for installation path, and one or more custom variables.
    /// </summary>
    [DataContract]
    public class ApplicationWithOverrideVariableGroup : EntityBase
    {
        private bool _enabled;
        private Application _application;
        private CustomVariableGroup _customVariableGroup;

        [DataMember]
        public bool Enabled
        {
            get { return this._enabled; }

            set
            {
                this._enabled = value;
                NotifyPropertyChanged(() => this.Enabled);
            }
        }

        [DataMember]
        public string ApplicationId { get; set; }  // For RavenDB, grrrr...

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
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

        [DataMember]
        public string CustomVariableGroupId { get; set; }  // For RavenDB, grrrr...

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
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
        public InstallationResultContainer Install(ApplicationServer applicationServer)
        {
            bool atLeastOneTaskFailed = false;
            int numberOfSuccessfulTasks = 0;
            InstallationResultContainer installationResultContainer = new InstallationResultContainer();
            try
            {
                // Note: We do a ToList() here because we get a "collection was modified" exception otherwise. The reason we
                //       get the exception is because, somewhere else in this processing, we make this call:
                //       CustomVariableGroupLogic.Get(application.Name)
                //       That method does a refresh on the CustomVariableGroup, which contains an app, which contains the tasks.
                //       Good times.
                foreach (TaskBase taskBase in this.Application.MainAndPrerequisiteTasks.ToList().OrderBy(task => task.Sequence))
                {
                    DateTime taskStartTime = DateTime.Now;

                    taskBase.Execute(applicationServer, this);

                    installationResultContainer.TaskDetails.Add(new TaskDetail(taskStartTime, DateTime.Now, taskBase.TaskDetails));

                    if (taskBase.TaskSucceeded == true) { numberOfSuccessfulTasks++; }

                    if (taskBase.TaskSucceeded == false)
                    {
                        atLeastOneTaskFailed = true;
                        if (taskBase.FailureCausesAllStop == 1) { break; }  // No more processing.
                    }
                }

                if (numberOfSuccessfulTasks < 1) { return FinalInstallationResultContainer(installationResultContainer, InstallationResult.Failure); }

                if (atLeastOneTaskFailed) { return FinalInstallationResultContainer(installationResultContainer, InstallationResult.PartialSuccess); }

                return FinalInstallationResultContainer(installationResultContainer, InstallationResult.Success);
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                return FinalInstallationResultContainer(installationResultContainer, InstallationResult.Failure);
            }
        }

        private static InstallationResultContainer FinalInstallationResultContainer(InstallationResultContainer container, InstallationResult result)
        {
            container.InstallationResult = result;
            return container;
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
