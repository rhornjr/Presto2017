using System;
using System.Xml.Serialization;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), Serializable]
    [XmlInclude( typeof( TaskMsi        ) )]
    [XmlInclude( typeof( TaskCopyFile   ) )]
    [XmlInclude( typeof( TaskDosCommand ) )]
    [XmlInclude( typeof( TaskXmlModify  ) )]
    public /* abstract */ class TaskBase : IComparable<TaskBase>
    {
        /// <summary>
        /// 
        /// </summary>
        public int? TaskItemId { get; /* internal */ set; }  // ToDo: Had to comment "internal" to get WPF two-way binding to work.

        /// <summary>
        /// 
        /// </summary>
        public byte FailureCausesAllStop { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TaskGroupId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TaskTypeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool TaskSucceeded { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TaskBase()
        {
            this.TaskItemId = null;
        }

        #region IComparable<TaskBase> Members

        /// <summary>
        /// Default sort TaskBase objects on its Sequence property
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public int CompareTo( TaskBase other )
        {
            return this.Sequence.CompareTo( other.Sequence );
        }

        #endregion
    }
}
