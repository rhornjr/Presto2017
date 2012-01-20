using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public IEnumerable<InstallationSummary> GetByServerAppAndGroup(ApplicationServer appServer, Entities.ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return ExecuteQuery<IEnumerable<InstallationSummary>>(() =>
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
            });
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
                return ExecuteQuery<IEnumerable<InstallationSummary>>(() =>
                {
                    IEnumerable<InstallationSummary> installationSummaries =
                        QueryAndCacheEtags(session => session.Query<InstallationSummary>()
                        .Include(x => x.ApplicationServerId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.ApplicationId)
                        .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupId)                        
                        .OrderByDescending(summary => summary.InstallationStart)                        
                        .Take(numberToRetrieve)
                        ).Cast<InstallationSummary>().ToList();

                    // Note: We use session.Load() below so that we get the information from the session, and not another trip to the DB.
                    foreach (InstallationSummary summary in installationSummaries)
                    {
                        summary.ApplicationServer =
                            QuerySingleResultAndCacheEtag(session => session.Load<ApplicationServer>(summary.ApplicationServerId))
                            as ApplicationServer;

                        summary.ApplicationWithOverrideVariableGroup.Application =
                            QuerySingleResultAndCacheEtag(session => session.Load<Application>(summary.ApplicationWithOverrideVariableGroup.ApplicationId))
                            as Application;

                        if (summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null) { continue; }

                        summary.ApplicationWithOverrideVariableGroup.CustomVariableGroup =
                            QuerySingleResultAndCacheEtag(session => session.Load<CustomVariableGroup>(summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId))
                            as CustomVariableGroup;
                    }

                    return installationSummaries;
                });
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("InstallationSummaryData.GetMostRecentByStartTime(): " + stopwatch.ElapsedMilliseconds);
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
