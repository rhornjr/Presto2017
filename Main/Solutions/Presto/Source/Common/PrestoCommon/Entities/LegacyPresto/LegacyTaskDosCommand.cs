using System;
using System.Xml.Serialization;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [XmlType("TaskDosCommand")]
    public class LegacyTaskDosCommand : LegacyTaskBase
    {
        #region [Public Properties]

        /// <summary>
        /// 
        /// </summary>
        public string DosExecutable { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Parameters { get; set; }

        #endregion
    }
}
