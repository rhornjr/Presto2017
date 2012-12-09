using System;
using System.Xml.Serialization;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [XmlType("TaskCopyFile")]
    public class LegacyTaskCopyFile : LegacyTaskBase
    {
        #region [Public Properties]

        /// <summary>
        /// 
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DestinationPath { get; set; }

        #endregion
    }
}
