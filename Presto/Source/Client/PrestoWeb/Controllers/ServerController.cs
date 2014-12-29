using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoWeb.EntityContainers;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://fs-12220", headers: "*", methods: "*")]
    public class ServerController : ApiController
    {
        public ApplicationServer Get(string id)
        {
            // Because RavenDB has a slash in the ID. The caller replaced it with ^^.
            id = id.Replace("^^", "/");

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                return prestoWcf.Service.GetServerById(id);
            }
        }

        public void InstallApp(AppServerAndAppWithGroup appServerAndAppWithGroup)
        {
            var server       = appServerAndAppWithGroup.Server;
            var appWithGroup = appServerAndAppWithGroup.AppWithGroup;

            var serverForceInstallation = new ServerForceInstallation(server, appWithGroup);
            var serverForceInstallations = new List<ServerForceInstallation>();
            serverForceInstallations.Add(serverForceInstallation);

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                prestoWcf.Service.SaveForceInstallations(serverForceInstallations);
            }
        }
    }
}