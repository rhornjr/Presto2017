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
    public class AppsController : ApiController
    {
        // The origin is the web server.
        [EnableCors(origins: "http://fs-6103:8048", headers: "*", methods: "*")]
        public IEnumerable<Application> Get()
        {
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.GetAllApplications(true).OrderBy(x => x.Name);
            }
        }
    }
}
