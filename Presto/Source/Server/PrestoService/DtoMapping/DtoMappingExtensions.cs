using System.Collections.Generic;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;

namespace PrestoWcfService.DtoMapping
{
    internal static class DtoMappingExtensions
    {
        #region [Applications]

        internal static IEnumerable<ApplicationDtoSlim> ToDtoSlim(this IEnumerable<Application> apps)
        {
            var slimApps = new List<ApplicationDtoSlim>();

            foreach (var app in apps)
            {
                var slimApp     = new ApplicationDtoSlim();
                slimApp.Id      = app.Id;
                slimApp.Name    = app.Name;
                slimApp.Version = app.Version;

                slimApps.Add(slimApp);
            }

            return slimApps;
        }

        #endregion

        #region [Servers]

        internal static IEnumerable<ApplicationServerDtoSlim> ToDtoSlim(this IEnumerable<ApplicationServer> servers)
        {
            var slimServers = new List<ApplicationServerDtoSlim>();

            foreach (var server in servers)
            {
                var slimServer                      = new ApplicationServerDtoSlim();
                slimServer.Id                       = server.Id;
                slimServer.Name                     = server.Name;
                slimServer.InstallationEnvironment = server.InstallationEnvironment.Name;

                slimServers.Add(slimServer);
            }

            return slimServers;
        }

        #endregion
    }
}
