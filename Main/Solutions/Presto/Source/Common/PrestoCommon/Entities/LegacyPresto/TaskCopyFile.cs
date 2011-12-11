using System;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TaskCopyFile : TaskBase
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
