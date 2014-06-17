using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://fs-6103:8048", headers: "*", methods: "*")]
    public class ServersController : ApiController
    {
        public IEnumerable<ApplicationServer> Get()
        {
            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                return prestoWcf.Service.GetAllServersSlim().OrderBy(x => x.Name);
            }
        }
    }
}
