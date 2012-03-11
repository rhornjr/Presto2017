using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class InstallationSummaryLogic
    {
        /// <summary>
        /// Gets the name of the by server.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appWithGroup">The app with group.</param>
        /// <returns></returns>
        public static IEnumerable<InstallationSummary> GetByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetByServerAppAndGroup(appServer, appWithGroup);
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public static IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTime(numberToRetrieve);
        }

        /// <summary>
        /// Saves the specified installation summary.
        /// </summary>
        /// <param name="installationSummary">The installation summary.</param>
        public static void Save(InstallationSummary installationSummary)
        {
            DataAccessFactory.GetDataInterface<IInstallationSummaryData>().Save(installationSummary);
        }
    }
}
