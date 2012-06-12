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
                    IEnumerable<ApplicationServer> appServers = QueryAndSetEtags(session =>
                        session.Query<ApplicationServer>()
                        .Include(x => x.CustomVariableGroupIds)
                        .Include(x => x.ApplicationIdsForAllAppWithGroups)
                        .Include(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                        .Include(x => x.CustomVariableGroupIdsForGroupsWithinApps)
                        .Take(int.MaxValue)
                        ).AsEnumerable().Cast<ApplicationServer>();

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
                ApplicationServer appServer = QuerySingleResultAndSetEtag(session =>
                    session.Query<ApplicationServer>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Include(x => x.ApplicationIdsForAllAppWithGroups)
                    .Include(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                    .Include(x => x.CustomVariableGroupIdsForGroupsWithinApps)
                    .Where(server => server.Name == serverName).FirstOrDefault())
                    as ApplicationServer;

                if (appServer != null) { HydrateApplicationServer(appServer); }

                return appServer;
            });            
        }

        /// <summary>
        /// Gets the object by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public ApplicationServer GetById(string id)
        {
            return ExecuteQuery<ApplicationServer>(() =>
            {
                ApplicationServer appServer = QuerySingleResultAndSetEtag(session =>
                    session.Query<ApplicationServer>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Include(x => x.ApplicationIdsForAllAppWithGroups)
                    .Include(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                    .Include(x => x.CustomVariableGroupIdsForGroupsWithinApps)
                    .Where(server => server.Id == id).FirstOrDefault())
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
                appServer.CustomVariableGroups.Add(QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(groupId)) as CustomVariableGroup);
            }

            foreach (ApplicationWithOverrideVariableGroup appGroup in appServer.ApplicationsWithOverrideGroup)
            {
                appGroup.Application = QuerySingleResultAndSetEtag(session => session.Load<Application>(appGroup.ApplicationId)) as Application;
                ApplicationData.HydrateApplication(appGroup.Application);
                if (appGroup.CustomVariableGroupId == null) { continue; }
                appGroup.CustomVariableGroup = QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(appGroup.CustomVariableGroupId)) as CustomVariableGroup;
            }

            foreach (ApplicationWithOverrideVariableGroup group in appServer.ApplicationWithGroupToForceInstallList)
            {
                group.Application =
                    QuerySingleResultAndSetEtag(session => session.Load<Application>(group.ApplicationId)) as Application;

                ApplicationData.HydrateApplication(group.Application);

                if (group.CustomVariableGroupId != null)
                {
                    group.CustomVariableGroup =
                        QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(group.CustomVariableGroupId))
                        as CustomVariableGroup;
                }
            }
        }

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        public void Save(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            applicationServer.ApplicationIdsForAllAppWithGroups       = new List<string>();
            applicationServer.CustomVariableGroupIdsForAllAppWithGroups = new List<string>();
            applicationServer.CustomVariableGroupIds                  = new List<string>();

            // For each group, set its ApplicationId and CustomVariableGroupId.
            
            applicationServer.CustomVariableGroupIdsForGroupsWithinApps = new List<string>();

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

                foreach (CustomVariableGroup customGroup in appGroup.Application.CustomVariableGroups)
                {
                    applicationServer.CustomVariableGroupIdsForGroupsWithinApps.Add(customGroup.Id);
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
