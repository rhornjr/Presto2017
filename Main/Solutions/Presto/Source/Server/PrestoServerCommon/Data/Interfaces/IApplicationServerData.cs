using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IApplicationServerData
    {
        IEnumerable<ApplicationServer> GetAll();

        ApplicationServer GetByName(string serverName);

        ApplicationServer GetById(string id);

        void Save(ApplicationServer applicationServer);

        IEnumerable<ServerForceInstallation> GetForceInstallationsByServerId(string serverId);

        void SaveForceInstallations(IEnumerable<ServerForceInstallation> serverForceInstallations);

        void RemoveForceInstallation(ServerForceInstallation forceInstallation);
    }
}
