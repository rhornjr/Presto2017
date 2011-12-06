using System;
using PrestoCommon.Enums;

namespace PrestoViewModel.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TaskTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the task.
        /// </summary>
        /// <value>
        /// The type of the task.
        /// </value>
        public TaskType TaskType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTypeAttribute"/> class.
        /// </summary>
        /// <param name="taskType">Type of the task.</param>
        public TaskTypeAttribute(TaskType taskType)
        {
            this.TaskType = taskType;
        }
    }
}
