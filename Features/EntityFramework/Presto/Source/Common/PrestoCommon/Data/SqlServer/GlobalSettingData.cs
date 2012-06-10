using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class GlobalSettingData : DataAccessLayerBase, IGlobalSettingData
    {
        public GlobalSetting GetItem()
        {
            return this.Database.GlobalSettings.FirstOrDefault();
        }

        public void Save(GlobalSetting globalSetting)
        {
            this.SaveChanges<GlobalSetting>(globalSetting);
        }
    }
}
