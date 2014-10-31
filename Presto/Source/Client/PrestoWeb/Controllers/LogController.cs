using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://fs-6103", headers: "*", methods: "*")]
    public class LogController : ApiController
    {
        public IEnumerable<LogMessage> Get()
        {
            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                return prestoWcf.Service.GetMostRecentLogMessagesByCreatedTime(50);
            }
        }
    }
}
