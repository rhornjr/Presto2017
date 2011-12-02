using System.Collections.ObjectModel;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// ApplicationServer entity
    /// </summary>
    public class ApplicationServer
    {
        private Collection<Application> _applications;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets the application.
        /// </summary>
        public Collection<Application> Applications
        {
            get
            {
                if (this._applications == null) { this._applications = new Collection<Application>(); }

                return this._applications;
            }
        }
    }
}
