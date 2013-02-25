using System;

namespace PrestoCommon.Entities
{
    public class ForceInstallation : EntityBase
    {
        private DateTime? _forceInstallationTime;
        private InstallationEnvironment _forceInstallEnvironment;

        public DateTime? ForceInstallationTime
        {
            get { return this._forceInstallationTime; }

            set
            {
                this._forceInstallationTime = value;
                NotifyPropertyChanged(() => this.ForceInstallationTime);
            }
        }

        public InstallationEnvironment ForceInstallEnvironment
        {
            get { return this._forceInstallEnvironment; }

            set
            {
                this._forceInstallEnvironment = value;
                NotifyPropertyChanged(() => this.ForceInstallEnvironment);
            }
        }

        public override string ToString()
        {
            return this.ForceInstallationTime + " - " + this.ForceInstallEnvironment.ToString();
        }
    }
}
