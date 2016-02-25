using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IInstallationSummaryService
    {
        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId);

        [OperationContract]
        void SaveInstallationSummary(InstallationSummary installationSummary);

        [OperationContract]
        IEnumerable<ServerForceInstallation> GetPendingInstallations();
    }
}
