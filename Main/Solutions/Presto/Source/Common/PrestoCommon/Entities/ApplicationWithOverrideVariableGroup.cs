using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Enums;
using Raven.Imports.Newtonsoft.Json;
using Xanico.Core;

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
        private PrestoObservableCollection<CustomVariableGroup> _customVariableGroups;

            /// <summary>
        /// For each installation, this is set to the installation start time. It is used as a unique
        /// indentifer for each installation so that it can be used in custom variables. The original
        /// need for this was to create backups during a deployment. That way one task can create a
        /// backup folder (say c:\backup\app\20130906.112004), and the rest of the tasks can also 
        /// refer to that same folder.
        /// The format of this property is: "yyyyMMdd.hhmmss".
        /// </summary>
        [JsonIgnore] // Only necessary in memory during a deployment
        public static string InstallationStartTimestamp { get; set; }

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

        /**************************************************************************************************
         * 
         * Note: The reason we have CustomVariableGroupId (singular) and CustomVariableGroupIds (plural)
         *       properties is because we only allowed for one CustomVariableGroup until November 2014.
         *       Now that we can have multiple CustomVariableGroups, we need both properties (at least
         *       until every instance in the DB has been updated to only use the plural version).
         * 
         **************************************************************************************************/

        /// <summary>
        /// The IDs to persist within RavenDB. These IDs should NOT be used in business logic; only in the
        /// data layer when getting and saving to RavenDB. Business logic will manipulate the
        /// CustomVariableGroups property, and that property and the IDs can get out of sync until
        /// the data layer handles it.
        /// </summary>
        [DataMember]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB, grrrr...

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public PrestoObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get { return this._customVariableGroups; }

            set
            {
                this._customVariableGroups = value;                
                NotifyPropertyChanged(() => this.CustomVariableGroups);
                NotifyPropertyChanged(() => this.CustomVariableGroupNames);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification="WCF serialization")]
        [JsonIgnore]  // We do not want RavenDB to serialize this.
        [DataMember]  // ... but we still want it to go over WCF
        public string CustomVariableGroupNames
        {
            get
            {
                if (CustomVariableGroups == null || CustomVariableGroups.Count < 1) { return string.Empty; }

                var stringBuilder = new StringBuilder();
                foreach (var cvg in CustomVariableGroups)
                {
                    stringBuilder.Append(cvg.Name + " | ");
                }

                // Change the length so we don't recognize the last three characters (" | ");
                stringBuilder.Length -= 3;

                return stringBuilder.ToString();
            }

            private set
            {
                // Do nothing. This is only here so the WCF serializer can properly construct this object.
            }
        }

        public static void SetInstallationStartTimestamp(DateTime dateTime)
        {
            InstallationStartTimestamp = dateTime.ToString("yyyyMMdd.HHmmss", CultureInfo.CurrentCulture);
        }

        private static InstallationResultContainer _installationResultContainer;
        private static bool _atLeastOneTaskFailedWhereFailureCausesAllStop = false;

        /// <summary>
        /// Installs this instance.
        /// </summary>
        public InstallationResultContainer Install(ApplicationServer applicationServer, DateTime installationStartTime, bool calledFromAppInstaller)
        {
            SetInstallationStartTimestamp(installationStartTime);

            // Before 23-Jun-2014, the only caller to this method was the AppInstaller. Since a new task (TaskApp) was added, it
            // also calls this method now. We only want to initialize a new container when called by AppInstaller. TaskApp will
            // call this method, but it won't do anything with the result. So, we just want to capture all of the task details
            // here, including the ones generated by TaskApp, then return the whole thing to AppInstaller.
            if (calledFromAppInstaller) { _installationResultContainer = new InstallationResultContainer(); }

            bool atLeastOneTaskFailed = false;
            int numberOfSuccessfulTasks = 0;

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

                    PossiblyAddTaskAppStartToInstallationResults(taskBase, taskStartTime);

                    taskBase.Execute(applicationServer, this);

                    _installationResultContainer.TaskDetails.Add(new TaskDetail(taskStartTime, DateTime.Now, taskBase.TaskDetails));

                    if (taskBase.TaskSucceeded == true) { numberOfSuccessfulTasks++; }

                    // The reason this is here is because we're trying to detect when failures occur within the TaskApp types.
                    if (_atLeastOneTaskFailedWhereFailureCausesAllStop) { break; }

                    if (taskBase.TaskSucceeded == false)
                    {
                        atLeastOneTaskFailed = true;
                        if (taskBase.FailureCausesAllStop == 1)
                        {
                            _atLeastOneTaskFailedWhereFailureCausesAllStop = true;
                            break;  // No more processing.
                        } 
                    }
                }

                return FinalInstallationResultContainer(_installationResultContainer, InstallationResult.Success, numberOfSuccessfulTasks, atLeastOneTaskFailed);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return FinalInstallationResultContainer(_installationResultContainer, InstallationResult.Failure, numberOfSuccessfulTasks, atLeastOneTaskFailed);
            }
            finally
            {
                // Clear this so we don't keep things hanging around in memory unnecessarily.
                if (calledFromAppInstaller) { _installationResultContainer = null; }
                if (calledFromAppInstaller) { _atLeastOneTaskFailedWhereFailureCausesAllStop = false; } // Reset for the next call
            }
        }

        private static void PossiblyAddTaskAppStartToInstallationResults(TaskBase taskBase, DateTime taskStartTime)
        {
            // If we're about to run a TaskApp, include that in our installation results.
            if (taskBase as TaskApp != null)
            {
                _installationResultContainer.TaskDetails.Add(new TaskDetail(taskStartTime, DateTime.Now,
                    "Starting a TaskApp: " + taskBase.Description));
            }
        }

        private static InstallationResultContainer FinalInstallationResultContainer(
            InstallationResultContainer container, InstallationResult result, int numberOfSuccessfulTasks, bool atLeastOneTaskFailed)
        {
            if (_atLeastOneTaskFailedWhereFailureCausesAllStop || numberOfSuccessfulTasks < 1)
            {
                container.InstallationResult = InstallationResult.Failure;
                return container;
            }

            if (atLeastOneTaskFailed)
            {
                container.InstallationResult = InstallationResult.PartialSuccess;
                return container;
            }

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

            if (this.CustomVariableGroups != null && this.CustomVariableGroups.Count > 0)
            {
                groupNameSuffix = " with " + this.CustomVariableGroupNames;
            }

            return appAndVersion + groupNameSuffix;
        }
    }
}
