using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoServer.Data.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace PrestoServer.Data.RavenDb
{
    public class InstallationSummaryData : DataAccessLayerBase, IInstallationSummaryData
    {
        /// <summary>
        /// Gets the by server name app version and group.
        /// </summary>
        /// <param name="appServer"></param>
        /// <param name="appWithGroup">The app with group.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }

            Expression<Func<InstallationSummary, bool>> whereClause;

            if (appWithGroup.CustomVariableGroups == null || appWithGroup.CustomVariableGroups.Count < 1)
            {
                whereClause = summary => summary.ApplicationServerId == appServer.Id &&
                    summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id &&
                    summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null &&
                    (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds == null
                      || summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.Count < 1);

                return ExecuteQuery<InstallationSummary>(() =>
                {
                    InstallationSummary installationSummary =
                        QuerySingleResultAndSetEtag(session => session.Query<InstallationSummary>()
                        .Include(x => x.ApplicationServerId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.ApplicationId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds)
                        .Customize(x => x.WaitForNonStaleResults())
                        .Where(whereClause)
                        .OrderByDescending(x => x.InstallationStart)
                        .FirstOrDefault()) as InstallationSummary;

                    HydrateInstallationSummary(installationSummary);

                    return installationSummary;
                });
            }

            // The where clause used to be generated in a private method, but RavenDB can't handle this last line:
            // summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.All(appWithGroup.CustomVariableGroupIds.Contains);
            // So now just get more installation summaries from RavenDB than we need, and implement that last line in memory.
            whereClause = summary => summary.ApplicationServerId == appServer.Id &&
                            summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id &&
                            summary.ApplicationWithOverrideVariableGroup != null &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds != null &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.Count == appWithGroup.CustomVariableGroupIds.Count;

            return ExecuteQuery<InstallationSummary>(() =>
            {
                var installationSummaries =
                    QueryAndSetEtags(session => session.Query<InstallationSummary>()
                    .Include(x => x.ApplicationServerId)
                    .Include(x => x.ApplicationWithOverrideVariableGroup.ApplicationId)
                    .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds)
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(whereClause)
                    .OrderByDescending(x => x.InstallationStart)
                    as IQueryable<EntityBase>);

                // Loop through the installation summaries to find the one that has the exact same CVG IDs
                // as appWithGroup.CustomVariableGroupIds.
                InstallationSummary theInstallationSummary = null;
                foreach (var summary in installationSummaries.ToList().Cast<InstallationSummary>())
                {
                    if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.All(appWithGroup.CustomVariableGroupIds.Contains))
                    {
                        theInstallationSummary = summary;
                    }
                }

                HydrateInstallationSummary(theInstallationSummary);

                return theInstallationSummary;
            });
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return GetMostRecentByStartTimeAndWhereClause(numberToRetrieve, _ => true);
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId)
        {
            return GetMostRecentByStartTimeAndWhereClause(numberToRetrieve, x => x.ApplicationServerId == serverId);
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId)
        {
            return GetMostRecentByStartTimeAndWhereClause(numberToRetrieve, x => x.ApplicationWithOverrideVariableGroup.ApplicationId == appId);
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId)
        {
            return GetMostRecentByStartTimeAndWhereClause(numberToRetrieve,
                x => x.ApplicationWithOverrideVariableGroup.ApplicationId == appId && x.ApplicationServerId == serverId);
        }

        private IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndWhereClause(int numberToRetrieve, Expression<Func<InstallationSummary, bool>> whereClause)
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
                        .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds)
                        .Customize(x => x.WaitForNonStaleResults())
                        .Where(whereClause)
                        .OrderByDescending(summary => summary.InstallationStartUtc)
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

            if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds == null
                || summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.Count < 1) { return; }

            if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroups == null)
            {
                summary.ApplicationWithOverrideVariableGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            }

            foreach (string groupId in summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds)
            {
                summary.ApplicationWithOverrideVariableGroup.CustomVariableGroups.Add(
                    QuerySingleResultAndSetEtag(session =>
                    {
                        if (session.Advanced.IsLoaded(groupId))
                        {
                            return session.Load<CustomVariableGroup>(groupId);
                        }
                        return null;  // Note: We can be missing a custom variable in the session because someone deleted it.
                        ;
                    })
                    as CustomVariableGroup);
            }
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
