﻿using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

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
            using (IDocumentSession session = Database.OpenSession())
            {
                IEnumerable<InstallationSummary> installationSummaryList =
                    session.Query<InstallationSummary>()
                    .Where(summary =>
                        summary.ApplicationServer.Name.ToUpperInvariant() == serverName.ToUpperInvariant()
                        && summary.ApplicationWithOverrideVariableGroup.Application.Name.ToUpperInvariant() == appWithGroup.Application.Name.ToUpperInvariant()
                        && summary.ApplicationWithOverrideVariableGroup.Application.Version.ToUpperInvariant() == appWithGroup.Application.Version.ToUpperInvariant());

                if (appWithGroup.CustomVariableGroup == null)
                {
                    return installationSummaryList.Where(summary => summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup == null);
                }

                return installationSummaryList.Where(summary =>
                    summary.ApplicationWithOverrideVariableGroup != null && summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup != null &&
                    summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup.Name == appWithGroup.CustomVariableGroup.Name);
            }
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<InstallationSummary>()
                    .OrderByDescending(summary => summary.InstallationStart)
                    .Take(numberToRetrieve);
            }
        }
    }
}
