
using System.Collections.ObjectModel;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;
namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    public class Application : IActivatable
    {
        [Transient]
        private IActivator _activator;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the release folder location.
        /// </summary>
        /// <value>
        /// The release folder location.
        /// </value>
        public string ReleaseFolderLocation { get; set; }

        /// <summary>
        /// Gets or sets the tasks.
        /// </summary>
        /// <value>
        /// The tasks.
        /// </value>
        public Collection<TaskBase> Tasks { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        public Application()
        {
            this.Tasks = new Collection<TaskBase>();
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
