using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
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
