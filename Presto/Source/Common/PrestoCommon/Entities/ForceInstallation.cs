using System;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class ForceInstallation : EntityBase
    {
        private DateTime? _forceInstallationTime;
        private DeploymentEnvironment _forceInstallationEnvironment;

        /// <summary>
        /// Gets or sets a value indicating whether to force an installation.
        /// Normally an app will only get installed on servers when there is a new version of the app.
        /// If we want the same version of the app installed again (like an update to QA), the set this
        /// to true so an installation occurs.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [force installation]; otherwise, <c>false</c>.
        /// </value>
        public DateTime? ForceInstallationTime
        {
            get { return this._forceInstallationTime; }

            set
            {
                this._forceInstallationTime = value;
                NotifyPropertyChanged(() => this.ForceInstallationTime);
            }
        }

        /// <summary>
        /// Gets or sets the force installation environment.
        /// </summary>
        /// <value>
        /// The force installation environment.
        /// </value>
        public DeploymentEnvironment ForceInstallationEnvironment
        {
            get { return this._forceInstallationEnvironment; }

            set
            {
                this._forceInstallationEnvironment = value;
                NotifyPropertyChanged(() => this.ForceInstallationEnvironment);
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
            return this.ForceInstallationTime + " - " + this.ForceInstallationEnvironment.ToString();
        }
    }
}
