using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class ForceInstallation : EntityBase
    {
        private DateTime? _forceInstallationTime;
        private InstallationEnvironment _forceInstallEnvironment;

        [DataMember]
        public DateTime? ForceInstallationTime
        {
            get { return this._forceInstallationTime; }

            set
            {
                this._forceInstallationTime = value;
                NotifyPropertyChanged(() => this.ForceInstallationTime);
            }
        }

        [DataMember]
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
