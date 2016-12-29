using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class GlobalController : ApiController
    {
        public GlobalSetting Get()
        {
            using(var prestoWcf = new PrestoWcf<IBaseService>())
            {
                var globalSetting = prestoWcf.Service.GetGlobalSettingItem();
                return globalSetting;
            }
        }

        public GlobalSetting Save(GlobalSetting globalSetting)
        {
            using(var prestoWcf = new PrestoWcf<IBaseService>())
            {
                var savedGlobalSetting = prestoWcf.Service.SaveGlobalSetting(globalSetting);
                return savedGlobalSetting;
            }
        }
    }
}
