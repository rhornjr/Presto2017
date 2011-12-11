using System;

namespace PrestoCommon.Entities.LegacyPresto
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msi"), Serializable]
    public class TaskMsi : TaskBase
    {
        /// <summary>
        /// 
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte PassiveInstall { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProductGuid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebSite")]
        public string IisWebSite { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte Install { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string InstallationLocation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TaskMsi()
        {
            // Some properties can be ignored, depending on whether it's an install or uninstall. So set those to empty strings.
            this.Path                 = string.Empty;
            this.FileName             = string.Empty;
            this.ProductGuid          = string.Empty;
            this.IisWebSite           = string.Empty;
            this.InstallationLocation = string.Empty;
        }
    }
}
