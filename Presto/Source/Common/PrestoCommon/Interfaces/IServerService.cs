using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        ApplicationServer GetServerById(string id);

        [OperationContract]
        IEnumerable<ApplicationServer> GetAllServers();

        [OperationContract]
        void InstallPrestoSelfUpdater(ApplicationServer appServer);

        [OperationContract]
        ApplicationServer SaveServer(ApplicationServer applicationServer);

        [OperationContract]
        void SaveForceInstallations(List<ServerForceInstallation> serverForceInstallations);

        [OperationContract]
        void RemoveForceInstallation(ServerForceInstallation forceInstallation);
    }
}
