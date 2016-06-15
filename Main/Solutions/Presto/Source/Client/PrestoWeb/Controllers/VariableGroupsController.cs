using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Xanico.Core;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]  // * See notes, below, for why
    public class VariableGroupsController : ApiController
    {
        //[Route("api/groups/getGroup")]
        public CustomVariableGroup Get(string id)
        {
            try
            {
                // Because RavenDB has a slash in the ID. The caller replaced it with ^^.
                id = id.Replace("^^", "/");

                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    return prestoWcf.Service.GetById(id);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Variable Group");
            }
        }

        public IEnumerable<CustomVariableGroup> Get()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    var groups = prestoWcf.Service.GetAllGroups().OrderBy(x => x.Name);
                    return groups;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Variable Groups");
            }
        }
    }
}
