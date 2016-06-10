using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoWeb.EntityContainers;
using Xanico.Core;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class ServerController : ApiController
    {
        public ApplicationServer Get(string id)
        {
            // Because RavenDB has a slash in the ID. The caller replaced it with ^^.
            id = id.Replace("^^", "/");

            try
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    return prestoWcf.Service.GetServerById(id);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Server");
            }
        }

        public void InstallApp(AppServerAndAppWithGroup appServerAndAppWithGroup)
        {
            try
            {
                var server = appServerAndAppWithGroup.Server;
                var appWithGroup = appServerAndAppWithGroup.AppWithGroup;

                var serverForceInstallation = new ServerForceInstallation(server, appWithGroup);
                var serverForceInstallations = new List<ServerForceInstallation>();
                serverForceInstallations.Add(serverForceInstallation);

                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    prestoWcf.Service.SaveForceInstallations(serverForceInstallations);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Installing App");
            }
        }

        [AcceptVerbs("POST")]
        [Route("api/server/save")]
        public ApplicationServer Save(ApplicationServer server)
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    return prestoWcf.Service.SaveServer(server);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Saving Server");
            }
        }
    }
}