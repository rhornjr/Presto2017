using System.Collections.Generic;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;

namespace PrestoWcfService.DtoMapping
{
    internal static class DtoMappingExtensions
    {
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
