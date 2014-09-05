using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://localhost:8048", headers: "*", methods: "*")]
    public class InstallsController : ApiController
    {
        public IEnumerable<InstallationSummary> Get()
        {
            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                return prestoWcf.Service.GetMostRecentByStartTime(50).OrderBy(x => x.InstallationStart).AsEnumerable();
            }
        }
    }
}