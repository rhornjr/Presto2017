using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class ResolveController : ApiController
    {
        /// <summary>
        /// Returns a list of resolved variables for a given <see cref="Application"/> and <see cref="ApplicationServer"/>.
        /// </summary>
        [HttpPost]
        public ResolvedVariablesContainer ResolveVariables(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            var app       = appAndServerAndOverrides.Application;
            var server    = appAndServerAndOverrides.Server;
            var overrides = appAndServerAndOverrides.Overrides;

            // Hydrate the app with trusted data (ie: don't use what was sent from the browser).
            Application hydratedApp;
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                hydratedApp = prestoWcf.Service.GetById(app.Id);
            }

            // Hydrate the server with trusted data (ie: don't use what was sent from the browser).
            ApplicationServer hydratedServer;
            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                hydratedServer = prestoWcf.Service.GetServerById(server.Id);
            }

            var appWithOverrides = new ApplicationWithOverrideVariableGroup();
            appWithOverrides.Application = hydratedApp;
            appWithOverrides.CustomVariableGroups = overrides;

            return VariableGroupResolver.Resolve(appWithOverrides, hydratedServer);
        }
    }
}
