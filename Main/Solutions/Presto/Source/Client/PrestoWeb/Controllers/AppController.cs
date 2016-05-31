using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
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

        public Application SaveApplication(Application application)
        {
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                return prestoWcf.Service.SaveApplication(application);
            }
        }

        [AcceptVerbs("POST")]
        [Route("api/app/getTaskExportFileContents")]
        public string GetTaskExportFileContents(List<TaskBase> selectedTasks)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var serializer = new NetDataContractSerializer();
                    serializer.Serialize(memoryStream, selectedTasks);
                    var streamAsString = Encoding.UTF8.GetString(memoryStream.ToArray());
                    return streamAsString;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Exporting Tasks");
            }
        }
    }
}