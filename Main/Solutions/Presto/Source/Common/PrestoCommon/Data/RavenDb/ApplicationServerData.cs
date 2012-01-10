using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationServer> GetAll()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                return ExecuteQuery<IEnumerable<ApplicationServer>>(() =>
                {
                    IEnumerable<ApplicationServer> appServers = QueryAndCacheEtags(session =>
                        session.Advanced.LuceneQuery<ApplicationServer>()
                        .Include(x => x.CustomVariableGroupIds)
                        .Include(x => x.ApplicationIdsForAllAppWithGroups)
                        .Include(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                        .Include(x => x.ApplicationWithGroupToForceInstall.ApplicationId)
                        .Include(x => x.ApplicationWithGroupToForceInstall.CustomVariableGroupId)
                        ).Cast<ApplicationServer>();

                    foreach (ApplicationServer appServer in appServers)
                    {
                        HydrateApplicationServer(appServer);
                    }

                    return appServers;
                });
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("ApplicationServerData.GetAll(): " + stopwatch.ElapsedMilliseconds);
            }
        }        

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public ApplicationServer GetByName(string serverName)
        {
            // Note: RavenDB queries are case-insensitive, so no ToUpper() conversion is necessary here.

            return ExecuteQuery<ApplicationServer>(() =>
            {
                ApplicationServer appServer = QuerySingleResultAndCacheEtag(session =>
                    session.Query<ApplicationServer>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Include(x => x.ApplicationIdsForAllAppWithGroups)
                    .Include(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                    .Include(x => x.ApplicationWithGroupToForceInstall.ApplicationId)
                    .Include(x => x.ApplicationWithGroupToForceInstall.CustomVariableGroupId)
                    .Where(server => server.Name == serverName).FirstOrDefault())
                    as ApplicationServer;

                if (appServer != null) { HydrateApplicationServer(appServer); }

                return appServer;
            });            
        }

        private static void HydrateApplicationServer(ApplicationServer appServer)
        {
            // Not using this, at least for now, because it increased the NumberOfRequests on the session...
            //appServer.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>(
            //    QueryAndCacheEtags(session => session.Load<CustomVariableGroup>(appServer.CustomVariableGroupIds)).Cast<CustomVariableGroup>());

            // ... however, this kept the NumberOfRequests to just one. Not sure why the difference.
            appServer.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>();
            foreach (string groupId in appServer.CustomVariableGroupIds)
            {                
                appServer.CustomVariableGroups.Add(QuerySingleResultAndCacheEtag(session => session.Load<CustomVariableGroup>(groupId)) as CustomVariableGroup);
            }

            foreach (ApplicationWithOverrideVariableGroup appGroup in appServer.ApplicationsWithOverrideGroup)
            {
                appGroup.Application = QuerySingleResultAndCacheEtag(session => session.Load<Application>(appGroup.ApplicationId)) as Application;
                if (appGroup.CustomVariableGroupId == null) { continue; }
                appGroup.CustomVariableGroup = QuerySingleResultAndCacheEtag(session => session.Load<CustomVariableGroup>(appGroup.CustomVariableGroupId)) as CustomVariableGroup;
            }

            if (appServer.ApplicationWithGroupToForceInstall != null)
            {
                appServer.ApplicationWithGroupToForceInstall.Application =
                    QuerySingleResultAndCacheEtag(session => session.Load<Application>(appServer.ApplicationWithGroupToForceInstall.ApplicationId)) as Application;
            }

            if (appServer.ApplicationWithGroupToForceInstall != null && appServer.ApplicationWithGroupToForceInstall.CustomVariableGroupId != null)
            {
                appServer.ApplicationWithGroupToForceInstall.CustomVariableGroup =
                    QuerySingleResultAndCacheEtag(session => session.Load<CustomVariableGroup>(appServer.ApplicationWithGroupToForceInstall.CustomVariableGroupId))
                    as CustomVariableGroup;
            }
        }

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        public void Save(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            applicationServer.ApplicationIdsForAllAppWithGroups         = new List<string>();
            applicationServer.CustomVariableGroupIdsForAllAppWithGroups = new List<string>();
            applicationServer.CustomVariableGroupIds                    = new List<string>();

            // For each group, set its ApplicationId and CustomVariableGroupId.
            foreach (ApplicationWithOverrideVariableGroup appGroup in applicationServer.ApplicationsWithOverrideGroup)
            {
                applicationServer.ApplicationIdsForAllAppWithGroups.Add(appGroup.Application.Id);
                appGroup.ApplicationId = appGroup.Application.Id;

                appGroup.CustomVariableGroupId = null;  // It's possible that the group was deleted, so clear it.
                if (appGroup.CustomVariableGroup != null)
                {
                    applicationServer.CustomVariableGroupIdsForAllAppWithGroups.Add(appGroup.CustomVariableGroup.Id);
                    appGroup.CustomVariableGroupId = appGroup.CustomVariableGroup.Id;
                }
            }

            // For each group, add its ID to the ID list.            
            foreach (CustomVariableGroup customGroup in applicationServer.CustomVariableGroups)
            {
                applicationServer.CustomVariableGroupIds.Add(customGroup.Id);
            }

            new GenericData().Save(applicationServer);
        }
    }
}
