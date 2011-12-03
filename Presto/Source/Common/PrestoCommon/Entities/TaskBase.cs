using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Base class for all tasks
    /// </summary>
    public abstract class TaskBase : ActivatableEntity
    {
        /// <summary>
        /// Gets or sets the failure causes all stop.
        /// </summary>
        /// <value>
        /// The failure causes all stop.
        /// </value>
        public byte FailureCausesAllStop { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the task.
        /// </summary>
        /// <value>
        /// The type of the task.
        /// </value>
        public TaskType TaskType { get; set; }

        /// <summary>
        /// Gets or sets the sequence.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        public int Sequence { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [task succeeded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [task succeeded]; otherwise, <c>false</c>.
        /// </value>
        public bool TaskSucceeded { get; set; }

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
            this.TaskType             = taskType;
            this.FailureCausesAllStop = failureCausesAllStop;
            this.Sequence             = sequence;
            this.TaskSucceeded        = taskSucceeded;
        }
    }
}
