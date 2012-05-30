using System;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
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
