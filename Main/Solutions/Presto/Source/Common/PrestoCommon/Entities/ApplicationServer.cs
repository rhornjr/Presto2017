using System.Collections.ObjectModel;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// ApplicationServer entity
    /// </summary>
    public class ApplicationServer : IActivatable
    {
        [Transient]
        private IActivator _activator;

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

        /// <summary>
        /// Activates the specified purpose.
        /// </summary>
        /// <param name="purpose">The purpose.</param>
        public void Activate(ActivationPurpose purpose)
        {
            if (this._activator != null)
            {
                this._activator.Activate(purpose);
            }
        }

        /// <summary>
        /// Binds the specified activator.
        /// </summary>
        /// <param name="activator">The activator.</param>
        public void Bind(IActivator activator)
        {
            if (_activator == activator) { return; }

            if (activator != null && null != _activator) { throw new System.InvalidOperationException(); }

            _activator = activator;
        }
    }
}
