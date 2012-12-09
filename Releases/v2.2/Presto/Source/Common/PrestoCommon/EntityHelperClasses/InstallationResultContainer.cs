using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoCommon.Enums;

namespace PrestoCommon.EntityHelperClasses
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationResultContainer
    {
        /// <summary>
        /// Gets or sets the installation result.
        /// </summary>
        /// <value>
        /// The installation result.
        /// </value>
        public InstallationResult InstallationResult { get; set; }

        /// <summary>
        /// Gets the task details.
        /// </summary>
        public List<TaskDetail> TaskDetails { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationResultContainer"/> class.
        /// </summary>
        public InstallationResultContainer()
        {
            this.TaskDetails = new List<TaskDetail>();
        }
    }
}
