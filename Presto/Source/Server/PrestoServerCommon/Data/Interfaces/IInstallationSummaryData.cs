using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IInstallationSummaryData
    {
        InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup);

        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve);

        void Save(InstallationSummary installationSummary);
    }
}
