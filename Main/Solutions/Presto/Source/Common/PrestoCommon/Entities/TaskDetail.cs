using System;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDetail : EntityBase
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        public string Details { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDetail"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="details">The details.</param>
        public TaskDetail(DateTime startTime, DateTime endTime, string details)
        {
            this.StartTime = startTime;
            this.EndTime   = endTime;
            this.Details   = details;
        }
    }
}
