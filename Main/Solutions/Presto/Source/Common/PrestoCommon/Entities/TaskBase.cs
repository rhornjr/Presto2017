using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using PrestoCommon.Enums;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    [KnownType("DerivedTypes")]
    [DataContract]
    public abstract class TaskBase : EntityBase
    {
        private string   _description;
        private byte     _failureCausesAllStop;
        private TaskType _taskType;
        private int      _sequence;
        private bool     _taskSucceeded;        
        private string   _taskDetails;

        [DataMember]
        public byte FailureCausesAllStop
        {
            get { return this._failureCausesAllStop; }

            set
            {
                this._failureCausesAllStop = value;
                NotifyPropertyChanged(() => this.FailureCausesAllStop);
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
        public TaskType PrestoTaskType
        {
            get { return this._taskType; }

            set
            {
                this._taskType = value;
                NotifyPropertyChanged(() => this.PrestoTaskType);
            }
        }

        [DataMember]
        public int Sequence
        {
            get { return this._sequence; }

            set
            {
                this._sequence = value;
                NotifyPropertyChanged(() => this.Sequence);
            }
        }

        [DataMember]
        public bool TaskSucceeded
        {
            get { return this._taskSucceeded; }

            set
            {
                this._taskSucceeded = value;
                NotifyPropertyChanged(() => this.TaskSucceeded);
            }
        }

        [DataMember]
        public string TaskDetails
        {
            get { return this._taskDetails; }

            set
            {
                this._taskDetails = value;
                NotifyPropertyChanged(() => this.TaskDetails);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBase"/> class.
        /// </summary>
        protected TaskBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskBase"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="taskType">Type of the task.</param>
        /// <param name="failureCausesAllStop">The failure causes all stop.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="taskSucceeded">if set to <c>true</c> [task succeeded].</param>
        protected TaskBase(string description, TaskType taskType, byte failureCausesAllStop, int sequence, bool taskSucceeded)
        {
            this.Description          = description;
            this.PrestoTaskType       = taskType;
            this.FailureCausesAllStop = failureCausesAllStop;
            this.Sequence             = sequence;
            this.TaskSucceeded        = taskSucceeded;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public abstract void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup);

        /// <summary>
        /// Gets the task properties. Each concrete task will add a string to the list that is the value of each property in the task.
        /// For example, for a copy file task, this would return three strings: SourcePath, SourceFileName, and DestinationPath.
        /// This is done so that custom variables can be resolved all at once.
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetTaskProperties();

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Description;
        }

        private static Type[] DerivedTypes()
        {
            return typeof(TaskBase).GetDerivedTypes(Assembly.GetExecutingAssembly()).ToArray();
        }
    }
}
