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
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]  // * See notes, below, for why this is necessary.
    public class AppsController : ApiController
    {
        //[Functionality(FunctionalityName = "GetApps")] // Disable for now because MES KLM Developers Contract also needs access.
        public IEnumerable<ApplicationDtoSlim> Get(bool includeArchivedApps)
        {
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.GetAllApplicationsSlim(includeArchivedApps).OrderBy(x => x.Name);
            }
        }
    }
}

/***************************************************************************************************************

 * Since the same server is serving both the HTML page and the Web API, EnableCors shouldn't be necessary.
 * However, this exception occurs if the EnableCors attribute doesn't exist:
 *   XMLHttpRequest cannot load http://webServerName/PrestoWeb/api/apps/. No 'Access-Control-Allow-Origin'
 *   header is present on the requested resource. Origin 'http://webServerName' is therefore not allowed access.
 *   
 * The GET request header shows this:
 *   Host: apps.firstsolar.com
 *   Origin: http://apps.firstsolar.com
 *   
 * Since those are different (not sure why, DNS issue maybe), that could be causing the need for the
 * EnableCors attribute 
 *
 * See this link for more details: http://stackoverflow.com/q/26680461/279516
 *
 ***************************************************************************************************************/