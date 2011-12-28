using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
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
                return session.Advanced.LuceneQuery<InstallationSummary>()
                    .OrderBy("InstallationStart")  // ToDo: This needs to be DESC
                    .Take(numberToRetrieve);
            }
        }
    }
}
