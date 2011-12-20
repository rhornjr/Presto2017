using System.Linq;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application can be installed multiple times on any given app server. Using the canonical exmaple, each
    /// instance needs to be the same as the others except for installation path, and one or more custom variables.
    /// </summary>
    public class ApplicationWithOverrideVariableGroup : EntityBase
    {
        private Application _application;
        private CustomVariableGroup _customVariableGroup;

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public Application Application
        {
            get { return this._application; }

            set
            {
                this._application = value;
                NotifyPropertyChanged(() => this.Application);
            }
        }

        /// <summary>
        /// Gets or sets the custom variable group.
        /// </summary>
        /// <value>
        /// The custom variable group.
        /// </value>
        public CustomVariableGroup CustomVariableGroup
        {
            get { return this._customVariableGroup; }

            set
            {
                this._customVariableGroup = value;
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
    }
}
