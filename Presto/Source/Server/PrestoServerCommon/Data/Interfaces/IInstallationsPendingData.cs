using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IInstallationsPendingData
    {
        IEnumerable<ServerForceInstallation> GetPending();
    }
}
