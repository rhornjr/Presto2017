using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class UtilityController : ApiController
    {
        public string GetServiceAddress()
        {
            return ConfigurationManager.AppSettings["prestoServiceAddress"];
        }
    }
}