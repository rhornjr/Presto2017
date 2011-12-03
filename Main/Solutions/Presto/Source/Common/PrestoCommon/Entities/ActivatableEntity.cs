using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Base class for entities that wish to participate in transparent persistence with db4o.
    /// With transparent persistence, it is not necessary to provide an update depth; db4o
    /// will simply handle persisting the object graph as necessary.
    /// </summary>
    public class ActivatableEntity : IActivatable
    {
        [Transient]
        private IActivator _activator;

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
