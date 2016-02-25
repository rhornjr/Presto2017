using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Logic
{
    public static class InstallationsPendingLogic
    {
        public static IEnumerable<ServerForceInstallation> GetPending()
        {
            return DataAccessFactory.GetDataInterface<IInstallationsPendingData>().GetPending();
        }
    }
}
