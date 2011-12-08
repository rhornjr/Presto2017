using System.Collections.ObjectModel;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    public class Application
    {
        private ObservableCollection<TaskBase> _tasks;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

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
        public bool ForceInstallation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            this.Tasks = new ObservableCollection<TaskBase>();
        }        
    }
}
