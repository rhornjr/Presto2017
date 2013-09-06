using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application, or product, that gets installed.
    /// </summary>
    [DataContract]
    public class Application : EntityBase
    {        
        private string _name;
        private ForceInstallation _forceInstallation;
        private ObservableCollection<TaskBase> _tasks;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private TaskVersionChecker _taskVersionChecker;

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
        public bool Archived { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
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
        [DataMember]
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

        /// <summary>
        /// Gets all of the tasks for an <see cref="Application"/>.
        /// Main tasks are this.Tasks. Prerequisite tasks are things like this.TaskVersionChecker,
        /// or anything new we might eventually add. Consuming code will want a way to access all
        /// tasks, so this property returns all of them.
        /// </summary>
        [JsonIgnore]  // We do not want RavenDB to serialize this. (We also don't need this to go over WCF.)
        public IEnumerable<TaskBase> MainAndPrerequisiteTasks
        {
            get
            {
                List<TaskBase> allTasks = new List<TaskBase>();

                if (this.TaskVersionChecker != null)
                {
                    // Put this before any other tasks, because TaskVersionChecker is a prerequisite.
                    this.TaskVersionChecker.Sequence = -1;
                    allTasks.Add(this.TaskVersionChecker);
                }

                allTasks.AddRange(this.Tasks);

                return allTasks;
            }
        }

        [DataMember]
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
