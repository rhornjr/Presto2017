using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// Data class for <see cref="GlobalSetting"/> entities.
    /// </summary>
    public class GlobalSettingData : DataAccessLayerBase, IGlobalSettingData
    {
        /// <summary>
        /// Gets the one and only <see cref="GlobalSetting"/> item.
        /// </summary>
        /// <returns></returns>
        public GlobalSetting GetItem()
        {
            return ExecuteQuery<GlobalSetting>(() =>
                {
                    GlobalSetting globalSetting = QuerySingleResultAndSetEtag(session =>
                        session.Query<GlobalSetting>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .FirstOrDefault()) as GlobalSetting;

                    return globalSetting;
                });
        }

        /// <summary>
        /// Saves the specified global setting.
        /// </summary>
        /// <param name="globalSetting">The global setting.</param>
        public void Save(GlobalSetting globalSetting)
        {
            new GenericData().Save(globalSetting);
        }
    }
}
