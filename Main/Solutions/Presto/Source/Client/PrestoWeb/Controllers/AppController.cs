using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
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

        [AcceptVerbs("POST")]
        [Route("api/app/importTasks")]
        public Application ImportTasks(AppAndTasksAsString appDto)
        {
            try
            {
                List<TaskBase> importedTasks = new List<TaskBase>();

                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.Write(appDto.TasksAsString);
                    var serializer = new NetDataContractSerializer();
                    writer.Flush();
                    memoryStream.Position = 0;
                    importedTasks = serializer.Deserialize(memoryStream) as List<TaskBase>;
                }

                var app = appDto.Application;
                int newSequenceNumber = app.Tasks.Count + 1;
                foreach (var task in importedTasks)
                {
                    task.Sequence = newSequenceNumber;
                    app.Tasks.Add(task);
                    newSequenceNumber++;
                }

                return SaveApplication(app);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Exporting Tasks");
            }
        }
    }
}