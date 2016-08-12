using System;
using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IInstallationSummaryData
    {
        InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup);
        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve, DateTime endDate);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId, DateTime endDate);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId, DateTime endDate);
        IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId, DateTime endDate);
        void Save(InstallationSummary installationSummary);
    }
}
