using System;
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
        /// <param name="appServer"></param>
        /// <param name="appWithGroup">The app with group.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetByServerAppAndGroup(ApplicationServer appServer, Entities.ApplicationWithOverrideVariableGroup appWithGroup)
        {
            IEnumerable<InstallationSummary> installationSummaryList = QueryAndCacheEtags(session => session.Query<InstallationSummary>()
                .Where(summary => summary.ApplicationServerId == appServer.Id
                        && summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id))
                .Cast<InstallationSummary>();

            if (appWithGroup.CustomVariableGroup == null)
            {
                return installationSummaryList.Where(summary => summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null);
            }

            return installationSummaryList.Where(summary =>
                summary.ApplicationWithOverrideVariableGroup != null && summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId != null &&
                summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == appWithGroup.CustomVariableGroupId);
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            IEnumerable<InstallationSummary> installationSummaries =
                QueryAndCacheEtags(session => session.Query<InstallationSummary>()
                .OrderByDescending(summary => summary.InstallationStart)
                .Take(numberToRetrieve)).Cast<InstallationSummary>();

            foreach (InstallationSummary summary in installationSummaries)
            {
                summary.ApplicationServer = DataAccessFactory.GetDataInterface<IApplicationServerData>().GetById(summary.ApplicationServerId);
                summary.ApplicationWithOverrideVariableGroup.Application =
                    DataAccessFactory.GetDataInterface<IApplicationData>().GetById(summary.ApplicationWithOverrideVariableGroup.ApplicationId);

                if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId != null)
                {
                    summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup =
                        DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetById(summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId);
                }
            }

            return installationSummaries;
        }

        /// <summary>
        /// Saves the specified installation summary.
        /// </summary>
        /// <param name="installationSummary">The installation summary.</param>
        public void Save(InstallationSummary installationSummary)
        {
            if (installationSummary == null) { throw new ArgumentNullException("installationSummary"); }

            installationSummary.ApplicationServerId = installationSummary.ApplicationServer.Id;

            DataAccessFactory.GetDataInterface<IGenericData>().Save(installationSummary);
        }
    }
}
