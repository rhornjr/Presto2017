using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]  // * See notes, below, for why
    public class PingController : ApiController
    {
        [Route("api/ping/latestRequest")]
        public PingRequest GetLatestRequest()
        {
            using (var prestoWcf = new PrestoWcf<IPingService>())
            {
                var latestPing = prestoWcf.Service.GetMostRecentPingRequest();
                return latestPing;
            }
        }

        [HttpPost]
        [Route("api/ping/responses")]
        public IEnumerable<PingResponse> GetResponses(PingRequest pingRequest)
        {
            using (var prestoWcf = new PrestoWcf<IPingService>())
            {
                var groups = prestoWcf.Service.GetAllForPingRequest(pingRequest);
                return groups;
            }
        }
    }
}
