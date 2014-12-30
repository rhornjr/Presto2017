using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class AppController : ApiController
    {
        public Application Get(string id)
        {
            // Because RavenDB has a slash in the ID. The caller replaced it with ^^.
            id = id.Replace("^^", "/");

            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.GetById(id);
            }
        }
    }
}