using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    public static class ApplicationServerLogic
    {
        public static IEnumerable<ApplicationServer> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetAll();
        }

        public static ApplicationServer GetByName(string serverName)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetByName(serverName);
        }

        public static ApplicationServer GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetById(id);
        }

        public static void Save(ApplicationServer applicationServer)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().Save(applicationServer);
        }

        #region [ServerForceInstallation]

        public static IEnumerable<ServerForceInstallation> GetForceInstallationsByServerId(string serverId)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetForceInstallationsByServerId(serverId);
        }

        public static void SaveForceInstallation(ServerForceInstallation serverForceInstallation)
        {
            List<ServerForceInstallation> serverForceInstallations = new List<ServerForceInstallation>();

            serverForceInstallations.Add(serverForceInstallation);

            SaveForceInstallations(serverForceInstallations);
        }

        public static void SaveForceInstallations(IEnumerable<ServerForceInstallation> serverForceInstallations)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().SaveForceInstallations(serverForceInstallations);
        }

        public static void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().RemoveForceInstallation(forceInstallation);
        }

        #endregion
    }
}
