using System.Collections.Generic;
using System.Collections.ObjectModel;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace PrestoCommon.Misc
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActivatableCollection<T> : Collection<T>, IActivatable
    {
        [Transient]
        private IActivator _activator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatableCollection&lt;T&gt;"/> class.
        /// </summary>
        public ActivatableCollection() : base() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivatableCollection&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public ActivatableCollection(IList<T> list) : base(list)
        {}

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
