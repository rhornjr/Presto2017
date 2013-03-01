using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    public class Application : EntityBase
    {
        private string _name;
        private ForceInstallation _forceInstallation;
        private ObservableCollection<TaskBase> _tasks;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private TaskVersionChecker _taskVersionChecker;

        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        public string Version { get; set; }

        public TaskVersionChecker TaskVersionChecker
        {
            get { return this._taskVersionChecker; }

            set
            {
                this._taskVersionChecker = value;
                this.NotifyPropertyChanged(() => this.TaskVersionChecker);
            }
        }

        [XmlIgnore]
        public ObservableCollection<TaskBase> Tasks
        {
            get
            {
                if (this._tasks == null) { this._tasks = new ObservableCollection<TaskBase>(); }
                return this._tasks;
            }

            private set
            {
                this._tasks = value;
            }
        }

        public ForceInstallation ForceInstallation
        {
            get { return this._forceInstallation; }

            set
            {
                this._forceInstallation = value;
                NotifyPropertyChanged(() => this.ForceInstallation);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                if (this._customVariableGroups == null) { this._customVariableGroups = new ObservableCollection<CustomVariableGroup>(); }

                return this._customVariableGroups;
            }

            set { this._customVariableGroups = value; }
        }

        public Application()
        {
            this.Tasks = new ObservableCollection<TaskBase>();
        }        

        public override string ToString()
        {
            return this.Name;
        }
    }
}
