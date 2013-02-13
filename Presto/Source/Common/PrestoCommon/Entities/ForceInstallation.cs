using System;

namespace PrestoCommon.Entities
{
    public class ForceInstallation : EntityBase
    {
        private DateTime? _forceInstallationTime;
        private InstallationEnvironment _forceInstallationEnvironment;

        public DateTime? ForceInstallationTime
        {
            get { return this._forceInstallationTime; }

            set
            {
                this._forceInstallationTime = value;
                NotifyPropertyChanged(() => this.ForceInstallationTime);
            }
        }

        public InstallationEnvironment ForceInstallationEnvironment
        {
            get { return this._forceInstallationEnvironment; }

            set
            {
                this._forceInstallationEnvironment = value;
                NotifyPropertyChanged(() => this.ForceInstallationEnvironment);
            }
        }

        public override string ToString()
        {
            return this.ForceInstallationTime + " - " + this.ForceInstallationEnvironment.ToString();
        }
    }
}
