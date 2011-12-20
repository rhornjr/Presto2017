using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Base class for all tasks
    /// </summary>
    public abstract class TaskBase : EntityBase
    {
        private string   _description;
        private byte     _failureCausesAllStop;
        private TaskType _taskType;
        private int      _sequence;
        private bool     _taskSucceeded;        

        /// <summary>
        /// Gets or sets the failure causes all stop.
        /// </summary>
        /// <value>
        /// The failure causes all stop.
        /// </value>
        public byte FailureCausesAllStop
        {
            get { return this._failureCausesAllStop; }

            set
            {
                this._failureCausesAllStop = value;
                NotifyPropertyChanged(() => this.FailureCausesAllStop);
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return this._description; }

            set
            {
                this._description = value;
                NotifyPropertyChanged(() => this.Description);
            }
        }

        /// <summary>
        /// Gets or sets the type of the task.
        /// </summary>
        /// <value>
        /// The type of the task.
        /// </value>
        public TaskType PrestoTaskType
        {
            get { return this._taskType; }

            set
            {
                this._taskType = value;
                NotifyPropertyChanged(() => this.PrestoTaskType);
            }
        }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public int Sequence
        {
            get { return this._sequence; }

            set
            {
                this._sequence = value;
                NotifyPropertyChanged(() => this.Sequence);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [task succeeded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [task succeeded]; otherwise, <c>false</c>.
        /// </value>
        public bool TaskSucceeded
        {
            get { return this._taskSucceeded; }

            set
            {
                this._taskSucceeded = value;
                NotifyPropertyChanged(() => this.TaskSucceeded);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBase"/> class.
        /// </summary>
        protected TaskBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBase"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="taskType">Type of the task.</param>
        /// <param name="failureCausesAllStop">The failure causes all stop.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="taskSucceeded">if set to <c>true</c> [task succeeded].</param>
        protected TaskBase(string description, TaskType taskType, byte failureCausesAllStop, int sequence, bool taskSucceeded)
        {
            this.Description          = description;
            this.PrestoTaskType             = taskType;
            this.FailureCausesAllStop = failureCausesAllStop;
            this.Sequence             = sequence;
            this.TaskSucceeded        = taskSucceeded;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute(ApplicationServer applicationServer, Application application);

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Description;
        }
    }
}
