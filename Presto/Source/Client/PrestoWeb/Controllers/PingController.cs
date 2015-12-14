using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]  // * See notes, below, for why
    public class PingController : ApiController
    {
        public IEnumerable<PingResponse> Get()
        {
            using (var prestoWcf = new PrestoWcf<IPingService>())
            {
                // ToDo: need ping request for this call.
                var groups = prestoWcf.Service.GetAllForPingRequest(null);
                return groups;
            }
        }
    }
}
