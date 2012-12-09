
namespace PrestoCommon.Entities
{
    /// <summary>
    /// Global settings for Presto.
    /// </summary>
    public class GlobalSetting : EntityBase
    {
        /// <summary>
        /// When set, no installations will be allowed to happen. The typical use for this is when
        /// deploying a change. This acts as sort of a safety: Set this to true, make the change,
        /// then after seeing that no servers started installing when they shouldn't, set this
        /// property back to false.
        /// </summary>
        public bool FreezeAllInstallations { get; set; }
    }
}
