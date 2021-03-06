﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server. localhost:4200 is Angular's local server.
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class ServersController : ApiController
    {
        public IEnumerable<ApplicationServerDtoSlim> Get()
        {
            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                return prestoWcf.Service.GetAllServersDtoSlim().OrderBy(x => x.Name);
            }
        }
    }
}
