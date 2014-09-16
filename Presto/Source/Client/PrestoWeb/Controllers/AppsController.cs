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
    [EnableCors(origins: "http://fs-12220", headers: "*", methods: "*")]
    public class AppsController : ApiController
    {
        public IEnumerable<Application> Get()
        {
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.GetAllApplications(true).OrderBy(x => x.Name);
            }
        }
    }
}
