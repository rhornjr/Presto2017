using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGlobalSettingData
    {
        /// <summary>
        ///  Gets the one and only <see cref="GlobalSetting"/> item.
        /// </summary>
        /// <returns></returns>
        GlobalSetting GetItem();

        /// <summary>
        /// Saves the specified global setting.
        /// </summary>
        /// <param name="globalSetting">The global setting.</param>
        void Save(GlobalSetting globalSetting);
    }
}
