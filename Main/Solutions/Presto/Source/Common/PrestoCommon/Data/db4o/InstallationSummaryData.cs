using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationSummaryData : DataAccessLayerBase, IInstallationSummaryData
    {
        /// <summary>
        /// Gets the by server name app version and group.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="appWithGroup">The app with group.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetByServerAppAndGroup(ApplicationServer appServer, Entities.ApplicationWithOverrideVariableGroup appWithGroup)
        {
            IEnumerable<InstallationSummary> installationSummaryList =
                from InstallationSummary summary in Database
                where summary.ApplicationServer.Name.ToUpperInvariant() == appServer.Name.ToUpperInvariant()
                  && summary.ApplicationWithOverrideVariableGroup.Application.Name.ToUpperInvariant() == appWithGroup.Application.Name.ToUpperInvariant()
                  && summary.ApplicationWithOverrideVariableGroup.Application.Version.ToUpperInvariant() == appWithGroup.Application.Version.ToUpperInvariant()
                select summary;

            Database.Ext().Refresh(installationSummaryList, 10);

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
        public IEnumerable<Entities.InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            IEnumerable<InstallationSummary> installationSummaryList = (from InstallationSummary summary in Database
                                                                        orderby summary.InstallationStart descending
                                                                        select summary).Take(numberToRetrieve);

            Database.Ext().Refresh(installationSummaryList, 10);

            return installationSummaryList;
        }

        /// <summary>
        /// Saves the specified installation summary.
        /// </summary>
        /// <param name="installationSummary">The installation summary.</param>
        public void Save(InstallationSummary installationSummary)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Save(installationSummary);
        }
    }
}
