using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationSummaryData : DataAccessLayerBase, IInstallationSummaryData
    {
        /// <summary>
        /// Gets the by server name app version and group.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <param name="appWithGroup">The app with group.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetByServerNameAppVersionAndGroup(string serverName, Entities.ApplicationWithOverrideVariableGroup appWithGroup)
        {
            IEnumerable<InstallationSummary> installationSummaryList = QueryAndCacheEtags(session => session.Query<InstallationSummary>()
                .Where(summary =>
                    summary.ApplicationServer.Name == serverName
                    && summary.ApplicationWithOverrideVariableGroup.Application.Name == appWithGroup.Application.Name
                    && summary.ApplicationWithOverrideVariableGroup.Application.Version == appWithGroup.Application.Version))
                    .Cast<InstallationSummary>();

            if (appWithGroup.CustomVariableGroup == null)
            {
                return installationSummaryList.Where(summary => summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup == null);
            }

            return installationSummaryList.Where(summary =>
                summary.ApplicationWithOverrideVariableGroup != null && summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup != null &&
                summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup.Name == appWithGroup.CustomVariableGroup.Name);
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return QueryAndCacheEtags(session => session.Query<InstallationSummary>()
                .OrderByDescending(summary => summary.InstallationStart)
                .Take(numberToRetrieve)).Cast<InstallationSummary>();
        }
    }
}
