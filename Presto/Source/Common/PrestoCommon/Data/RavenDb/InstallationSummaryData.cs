using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client.Linq;

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
        public InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return ExecuteQuery<InstallationSummary>(() =>
            {
                Expression<Func<InstallationSummary, bool>> whereClause = GetWhereClause(appServer, appWithGroup);

                InstallationSummary installationSummary =
                    QuerySingleResultAndSetEtag(session => session.Query<InstallationSummary>()
                    .Include(x => x.ApplicationServerId)
                    .Include(x => x.ApplicationWithOverrideVariableGroup.ApplicationId)
                    .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupId)
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(whereClause)
                    .OrderByDescending(x => x.InstallationStart)
                    .FirstOrDefault()) as InstallationSummary;

                HydrateInstallationSummary(installationSummary);

                return installationSummary;
            });
        }

        private static Expression<Func<InstallationSummary, bool>> GetWhereClause(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup.CustomVariableGroup == null)
            {
                return summary => summary.ApplicationServerId == appServer.Id &&
                            summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null;
            }

            return summary => summary.ApplicationServerId == appServer.Id &&
                            summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id &&
                            summary.ApplicationWithOverrideVariableGroup != null &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId != null &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == appWithGroup.CustomVariableGroupId;
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // We want to return IEnumerable...
                return ExecuteQuery<IEnumerable<InstallationSummary>>(() =>
                {
                    // ... however we must use IQueryable here so OrderBy() and Take() happen on the RavenDB end,
                    // and not in memory here. Actually, this isn't true. The big difference was between using a
                    // LuceneQuery and just Query. When using a LuceneQuery, the OrderByDescending() and Take()
                    // methods were Enumerable methods, and therefore didn't work correctly. When using Query,
                    // the OrderByDescending() and Take() were Queryable methods, and worked correctly. So, it
                    // looks like we could have used IEnumerable everywhere, as long as we used Query here.
                    IQueryable<EntityBase> installationSummaries =
                        QueryAndSetEtags(session => session.Query<InstallationSummary>()
                        .Include(x => x.ApplicationServerId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.ApplicationId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupId)
                        .Customize(x => x.WaitForNonStaleResults())
                        .OrderByDescending(summary => summary.InstallationStart)
                        .Take(numberToRetrieve)
                        );

                    HydrateInstallationSummaries(installationSummaries);

                    return installationSummaries.AsEnumerable().Cast<InstallationSummary>();
                });
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("InstallationSummaryData.GetMostRecentByStartTime(): " + stopwatch.ElapsedMilliseconds);
            }
        }

        private static void HydrateInstallationSummaries(IQueryable<EntityBase> installationSummaries)
        {
            // Note: We use session.Load() below so that we get the information from the session, and not another trip to the DB.
            foreach (InstallationSummary summary in installationSummaries)
            {
                HydrateInstallationSummary(summary);
            }
        }

        private static void HydrateInstallationSummary(InstallationSummary summary)
        {
            if (summary == null) { return; }

            summary.ApplicationServer =
                QuerySingleResultAndSetEtag(session => session.Load<ApplicationServer>(summary.ApplicationServerId))
                as ApplicationServer;

            summary.ApplicationWithOverrideVariableGroup.Application =
                QuerySingleResultAndSetEtag(session => session.Load<Application>(summary.ApplicationWithOverrideVariableGroup.ApplicationId))
                as Application;

            if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null) { return; }

            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup =
                QuerySingleResultAndSetEtag(session =>
                {
                    if (session.Advanced.IsLoaded(summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId))
                    {
                        return session.Load<CustomVariableGroup>(summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId);
                    }
                    return null;  // Note: We can be missing a custom variable in the session because someone deleted it.
                    ;
                })
                as CustomVariableGroup;
        }

        /// <summary>
        /// Saves the specified installation summary.
        /// </summary>
        /// <param name="installationSummary">The installation summary.</param>
        public void Save(InstallationSummary installationSummary)
        {
            if (installationSummary == null) { throw new ArgumentNullException("installationSummary"); }

            installationSummary.ApplicationServerId = installationSummary.ApplicationServer.Id;

            new GenericData().Save(installationSummary);
        }       
    }
}
