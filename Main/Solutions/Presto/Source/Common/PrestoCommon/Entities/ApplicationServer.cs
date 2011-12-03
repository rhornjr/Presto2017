using System.Collections.ObjectModel;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// ApplicationServer entity
    /// </summary>
    public class ApplicationServer : ActivatableEntity
    {
        private Collection<Application> _applications;
        private Collection<CustomVariableGroup> _customVariableGroups;

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

        /// <summary>
        /// Gets the custom variable groups.
        /// </summary>
        public Collection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                if (this._customVariableGroups == null) { this._customVariableGroups = new Collection<CustomVariableGroup>(); }

                return this._customVariableGroups;
            }
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
