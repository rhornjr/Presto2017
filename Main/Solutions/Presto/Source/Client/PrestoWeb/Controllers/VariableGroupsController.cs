using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
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

        [AcceptVerbs("POST")]
        [Route("api/variableGroups/save")]
        public CustomVariableGroup Save(CustomVariableGroup group)
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    return prestoWcf.Service.SaveGroup(group);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Saving Variable Group");
            }
        }

        [AcceptVerbs("POST")]
        [Route("api/variableGroups/delete")]
        public void Delete(CustomVariableGroup group)
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    prestoWcf.Service.DeleteGroup(group);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Deleting Variable Group");
            }
        }

        [AcceptVerbs("GET")]
        [Route("api/variableGroups/encrypt")]
        public string Encrypt(string valueToEncrypt)
        {
            try
            {
                var encryptedValue = AesCrypto.Encrypt(valueToEncrypt);
                return encryptedValue;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Encrypting Value");
            }
        }
    }
}
