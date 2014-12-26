using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://fs-12220", headers: "*", methods: "*")]  // * See notes, below, for why this is necessary.
    public class AppsController : ApiController
    {
        public IEnumerable<ApplicationDtoSlim> Get()
        {
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.GetAllApplicationsSlim().OrderBy(x => x.Name);
            }
        }
    }
}

/***************************************************************************************************************

 * Since the same server is serving both the HTML page and the Web API, EnableCors shouldn't be necessary.
 * However, this exception occurs if the EnableCors attribute doesn't exist:
 *   XMLHttpRequest cannot load http://webServerName/PrestoWebApi/api/apps/. No 'Access-Control-Allow-Origin'
 *   header is present on the requested resource. Origin 'http://webServerName' is therefore not allowed access.
 *   
 * The GET request header shows this:
 *   Host: fs-12220.fs.local
 *   Origin: http://fs-12220
 *   
 * Since those are different (not sure why, DNS issue maybe), that could be causing the need for the
 * EnableCors attribute 
 *
 * See this link for more details: http://stackoverflow.com/q/26680461/279516
 *
 ***************************************************************************************************************/