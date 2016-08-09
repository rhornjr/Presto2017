using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Xanico.Core;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class LogController : ApiController
    {
        public IEnumerable<LogMessage> Get()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    return prestoWcf.Service.GetMostRecentLogMessagesByCreatedTime(50);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Log Messages");
            }
        }

        [AcceptVerbs("POST")]
        [Route("api/log/saveLogMessage")]
        public void SaveLogMessage(string message)
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    prestoWcf.Service.SaveLogMessage(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Saving Log Message");
            }
        }
    }
}
