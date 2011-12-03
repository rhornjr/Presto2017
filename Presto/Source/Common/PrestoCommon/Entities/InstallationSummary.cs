using System;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// A record of an installation for an <see cref="Application"/> on an <see cref="ApplicationServer"/>
    /// </summary>
    public class InstallationSummary : ActivatableEntity
    {
        /// <summary>
        /// Gets or sets the application server.
        /// </summary>
        /// <value>
        /// The application server.
        /// </value>
        public ApplicationServer ApplicationServer { get; set; }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public Application Application { get; set; }

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
        /// Gets or sets the installation status.
        /// </summary>
        /// <value>
        /// The installation status.
        /// </value>
        public InstallationSummary InstallationResult { get; set; }
    }
}
