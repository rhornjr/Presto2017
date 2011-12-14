using System;
using System.Collections.ObjectModel;
using System.Linq;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    public class Application : NotifyPropertyChangedBase
    {
        private string _name;
        private ObservableCollection<TaskBase> _tasks;
        private DateTime? _forceInstallationTime;
        private DeploymentEnvironment _forceInstallationEnvironment;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the release folder location.
        /// </summary>
        /// <value>
        /// The release folder location.
        /// </value>
        public string ReleaseFolderLocation { get; set; }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public ObservableCollection<TaskBase> Tasks
        {
            get
            {
                if (this._tasks == null) { this._tasks = new ObservableCollection<TaskBase>(); }
                return this._tasks;
            }

            private set
            {
                this._tasks = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to force an installation.
        /// Normally an app will only get installed on servers when there is a new version of the app.
        /// If we want the same version of the app installed again (like an update to QA), the set this
        /// to true so an installation occurs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [force installation]; otherwise, <c>false</c>.
        /// </value>
        public DateTime? ForceInstallationTime
        {
            get { return this._forceInstallationTime; }

            set
            {
                this._forceInstallationTime = value;
                NotifyPropertyChanged(() => this.ForceInstallationTime);
            }
        }

        /// <summary>
        /// Gets or sets the force installation environment.
        /// </summary>
        /// <value>
        /// The force installation environment.
        /// </value>
        public DeploymentEnvironment ForceInstallationEnvironment
        {
            get { return this._forceInstallationEnvironment; }

            set
            {
                this._forceInstallationEnvironment = value;
                NotifyPropertyChanged(() => this.ForceInstallationEnvironment);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            this.Tasks = new ObservableCollection<TaskBase>();
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
            foreach (TaskBase task in this.Tasks.ToList())
            {
                task.Execute(applicationServer, this);

                if (task.TaskSucceeded == true) { numberOfSuccessfulTasks++; }

                if (task.TaskSucceeded == false)
                {
                    atLeastOneTaskFailed = true;
                    if (task.FailureCausesAllStop == 1) { break; }  // No more processing.
                }
            }

            if (numberOfSuccessfulTasks < 1) { return InstallationResult.Failure; }

            if (atLeastOneTaskFailed) { return InstallationResult.PartialSuccess; }

            return InstallationResult.Success;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
