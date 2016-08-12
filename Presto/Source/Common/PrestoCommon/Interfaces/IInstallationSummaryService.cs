using System;
using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IInstallationSummaryService
    {
        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve, DateTime endDate);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId, DateTime endDate);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId, DateTime endDate);

        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId, DateTime endDate);

        [OperationContract]
        void SaveInstallationSummary(InstallationSummary installationSummary);

        [OperationContract]
        IEnumerable<ServerForceInstallation> GetPendingInstallations();
    }
}
