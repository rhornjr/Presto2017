using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;

namespace PrestoServer.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class GlobalSettingLogic
    {
        /// <summary>
        /// Gets the one and only <see cref="GlobalSetting"/> item.
        /// </summary>
        /// <returns></returns>
        public static GlobalSetting GetItem()
        {
            return DataAccessFactory.GetDataInterface<IGlobalSettingData>().GetItem();
        }

        /// <summary>
        /// Saves the specified global setting.
        /// </summary>
        /// <param name="globalSetting">The global setting.</param>
        public static void Save(GlobalSetting globalSetting)
        {
            try
            {
                DataAccessFactory.GetDataInterface<IGlobalSettingData>().Save(globalSetting);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, "Global setting");
                throw;
            }
        }
    }
}
