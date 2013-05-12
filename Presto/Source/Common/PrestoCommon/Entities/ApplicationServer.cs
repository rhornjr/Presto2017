using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using PrestoCommon.Enums;
using PrestoCommon.Misc;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class ApplicationServer : EntityBase
    {
        private string _name;
        private string _description;
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _applicationsWithOverrideGroup;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private List<ServerForceInstallation> _forceInstallationsToDo;

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public List<ServerForceInstallation> ForceInstallationsToDo
        {
            get { return _forceInstallationsToDo; }
            set { _forceInstallationsToDo = value; }
        }

        [DataMember]
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        [DataMember]
        public string Description
        {
            get { return this._description; }

            set
            {
                this._description = value;
                NotifyPropertyChanged(() => this.Description);
            }
        }

        [DataMember]
        public bool EnableDebugLogging { get; set; }

        [DataMember]
        public string InstallationEnvironmentId { get; set; }

        // ToDo: Remove this after migrating the data to InstallationEnvironment
        [DataMember]
        public DeploymentEnvironment DeploymentEnvironment { get; set; }

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public InstallationEnvironment InstallationEnvironment { get; set; }

        [DataMember]
        public ObservableCollection<ApplicationWithOverrideVariableGroup> ApplicationsWithOverrideGroup
        {
            get
            {
                if (this._applicationsWithOverrideGroup == null) { this._applicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>(); }

                return this._applicationsWithOverrideGroup;
            }

            set { this._applicationsWithOverrideGroup = value; }
        }

        [DataMember]
        public List<string> ApplicationIdsForAllAppWithGroups { get; set; }  // For RavenDB

        [DataMember]
        public List<string> CustomVariableGroupIdsForAllAppWithGroups { get; set; }  // For RavenDB

        [DataMember]
        public List<string> CustomVariableGroupIdsForGroupsWithinApps { get; set; }  // For RavenDB        

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                if (this._customVariableGroups == null) { this._customVariableGroups = new ObservableCollection<CustomVariableGroup>(); }

                return this._customVariableGroups;
            }

            set { this._customVariableGroups = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Finds the matching <see cref="ApplicationWithOverrideVariableGroup"/> from the force install list.
        /// </summary>
        /// <param name="appWithGroup">The <see cref="ApplicationWithOverrideVariableGroup"/></param>
        /// <returns>If a match is found, returns the <see cref="ApplicationWithOverrideVariableGroup"/> instance.
        ///          If no match is found, returns null.
        /// </returns>
        public ServerForceInstallation GetFromForceInstallList(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return CommonUtility.GetAppWithGroup(this.ForceInstallationsToDo, appWithGroup);
        }
    }
}
