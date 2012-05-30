using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// A record of an installation for an <see cref="ApplicationWithOverrideVariableGroup"/> on an <see cref="ApplicationServer"/>
    /// </summary>
    public class InstallationSummary : EntityBase
    {
        /// <summary>
        /// Gets or sets the application server id.
        /// </summary>
        /// <value>
        /// The application server id.
        /// </value>
        public string ApplicationServerId { get; set; }

        /// <summary>
        /// Gets or sets the application server.
        /// </summary>
        /// <value>
        /// The application server.
        /// </value>
        [JsonIgnore]
        public ApplicationServer ApplicationServer { get; set; }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public ApplicationWithOverrideVariableGroup ApplicationWithOverrideVariableGroup { get; set; }

        /// <summary>
        /// Gets or sets the installation start.
        /// </summary>
        /// <value>
        /// The installation start.
        /// </value>
        public DateTime InstallationStart { get; set; }

        /// <summary>
        /// Gets or sets the installation end.
        /// </summary>
        /// <value>
        /// The installation end.
        /// </value>
        public DateTime InstallationEnd { get; set; }

        /// <summary>
        /// Gets or sets the installation result.
        /// </summary>
        /// <value>
        /// The installation result.
        /// </value>
        public InstallationResult InstallationResult { get; set; }

        public int InstallationResultId  // Because EF doesn't natively support enums
        {
            get { return (int)this.InstallationResult; }
            set { this.InstallationResult = (InstallationResult)value; }
        }

        /// <summary>
        /// Gets or sets the task details.
        /// </summary>
        /// <value>
        /// The task details.
        /// </value>
        public List<TaskDetail> TaskDetails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationSummary"/> class.
        /// </summary>
        /// <param name="applicationWithOverrideVariableGroup">The application.</param>
        /// <param name="applicationServer">The application server.</param>
        /// <param name="startTime">The start time.</param>
        public InstallationSummary(ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup, ApplicationServer applicationServer, DateTime startTime)
        {
            this.ApplicationWithOverrideVariableGroup = applicationWithOverrideVariableGroup;
            this.ApplicationServer                    = applicationServer;
            this.InstallationStart                    = startTime;
        }
    }
}
