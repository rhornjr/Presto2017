using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IInstallationSummaryData
    {
        InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup);
        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId);
        void Save(InstallationSummary installationSummary);
    }
}
