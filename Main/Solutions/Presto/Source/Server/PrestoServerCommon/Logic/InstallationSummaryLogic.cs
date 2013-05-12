using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Logic
{
    public static class InstallationSummaryLogic
    {
        public static InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByServerAppAndGroup(appServer, appWithGroup);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTime(numberToRetrieve);
        }

        public static void Save(InstallationSummary installationSummary)
        {
            DataAccessFactory.GetDataInterface<IInstallationSummaryData>().Save(installationSummary);
        }
    }
}
