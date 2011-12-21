using System.Collections.ObjectModel;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    public class Application : EntityBase
    {
        private string _name;
        private ForceInstallation _forceInstallation;
        private ObservableCollection<TaskBase> _tasks;        

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
        /// Gets or sets the force installation.
        /// </summary>
        /// <value>
        /// The force installation.
        /// </value>
        public ForceInstallation ForceInstallation
        {
            get { return this._forceInstallation; }

            set
            {
                this._forceInstallation = value;
                NotifyPropertyChanged(() => this.ForceInstallation);
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
