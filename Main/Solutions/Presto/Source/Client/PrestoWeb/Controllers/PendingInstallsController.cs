using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class PendingInstallsController : ApiController
    {
        public IEnumerable<ServerForceInstallation> Get()
        {
            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                return prestoWcf.Service.GetPendingInstallations();
            }
        }
    }
}