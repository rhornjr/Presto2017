using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoServer.Data.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace PrestoServer.Data.RavenDb
{
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        private static readonly object _locker = new object();

        public IEnumerable<ApplicationServer> GetAll(bool includeArchivedApps)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                return ExecuteQuery<IEnumerable<ApplicationServer>>(() =>
                {
                    IEnumerable<ApplicationServer> appServers = QueryAndSetEtags(session =>
                        session.Query<ApplicationServer>()
                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIds))
                        .Customize(x => x.Include<ApplicationServer>(y => y.ApplicationIdsForAllAppWithGroups))
                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForAllAppWithGroups))
                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForGroupsWithinApps))
                        .Customize(x => x.Include<ApplicationServer>(y => y.InstallationEnvironmentId))
                        .Take(int.MaxValue)
                        ).AsEnumerable().Cast<ApplicationServer>();

                    if (includeArchivedApps)
                    {
                        foreach (var server in appServers) { HydrateApplicationServer(server); }
                        return appServers;
                    }

                    // Note: We can't put this WHERE clause in the above query because the Archived
                    //       property was added after all of the other properties. It won't exist
                    //       for all apps, so we can't query by that property.
                    //       http://stackoverflow.com/a/11644645/279516
                    var serversNotArchived = appServers.Where(x => x.Archived != true);
                    foreach (ApplicationServer appServer in serversNotArchived) { HydrateApplicationServer(appServer); }
                    return serversNotArchived;
                });
            }
            finally
            {
                stopwatch.Stop();
                Debug.WriteLine("ApplicationServerData.GetAll(): " + stopwatch.ElapsedMilliseconds);
            }
        }

        public IEnumerable<ApplicationServer> GetAllSlim()
        {
            return ExecuteQuery<IEnumerable<ApplicationServer>>(() =>
            {
                IEnumerable<ApplicationServer> appServers = QueryAndSetEtags(session =>
                    session.Query<ApplicationServer>()
                    .Customize(x => x.Include<ApplicationServer>(y => y.InstallationEnvironmentId))
                    .Take(int.MaxValue)
                    ).AsEnumerable().Cast<ApplicationServer>();

                foreach (ApplicationServer appServer in appServers)
                {
                    appServer.InstallationEnvironment = QuerySingleResultAndSetEtag(session =>
                        session.Load<InstallationEnvironment>(appServer.InstallationEnvironmentId)) as InstallationEnvironment;
                }

                return appServers;
            });
        }

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
                           .Include(x => x.InstallationEnvironmentId)
                           .Customize(x => x.WaitForNonStaleResults())
                           .FirstOrDefault(server => server.Name == serverName))
                    as ApplicationServer;

                if (appServer != null) { HydrateApplicationServer(appServer); }

                return appServer;
            });            
        }

        public ApplicationServer GetById(string id)
        {
            return ExecuteQuery<ApplicationServer>(() =>
            {
                ApplicationServer appServer = QuerySingleResultAndSetEtag(session =>
                    session.Query<ApplicationServer>()
                           .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIds))
                           .Customize(x => x.Include<ApplicationServer>(y => y.ApplicationIdsForAllAppWithGroups))
                           .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForAllAppWithGroups))
                           .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForGroupsWithinApps))
                           .Customize(x => x.Include<ApplicationServer>(y => y.InstallationEnvironmentId))
                           .FirstOrDefault(server => server.Id == id))
                        as ApplicationServer;

                if (appServer != null) { HydrateApplicationServer(appServer); }

                return appServer;
            });
        }

        private static void HydrateApplicationServer(ApplicationServer appServer)
        {
            lock (_locker)  // prevent any thread silliness
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
                    LoadAppGroupCustomVariableGroups(appGroup);
                }

                appServer.InstallationEnvironment = QuerySingleResultAndSetEtag(session =>
                    session.Load<InstallationEnvironment>(appServer.InstallationEnvironmentId)) as InstallationEnvironment;
            }
        }

        private static void LoadAppGroupCustomVariableGroups(ApplicationWithOverrideVariableGroup appGroup)
        {
            if (appGroup.CustomVariableGroupId == null && (appGroup.CustomVariableGroupIds == null || appGroup.CustomVariableGroupIds.Count < 1))
            {
                return;
            }

            // Since both properties, CustomVariableGroupId (singular) and CustomVariableGroupIds (plural) can have data,
            // we'll put everything in CustomVariableGroupIds first.
            if (appGroup.CustomVariableGroupIds == null) { appGroup.CustomVariableGroupIds = new List<string>(); }

            if (appGroup.CustomVariableGroupId != null && !appGroup.CustomVariableGroupIds.Contains(appGroup.CustomVariableGroupId))
            {
                appGroup.CustomVariableGroupIds.Add(appGroup.CustomVariableGroupId);
            }

            // Store the CVG Ids in a different variable since the ones within the app group get reset.
            var copyOfCvgIds = new List<string>(appGroup.CustomVariableGroupIds);

            appGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();

            // Now that we have the IDs in one property, loop through the IDs and load the groups.
            foreach (var groupId in copyOfCvgIds)
            {
                var groupLoadedFromSession = QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(groupId)) as CustomVariableGroup;
                appGroup.CustomVariableGroups.Add(groupLoadedFromSession);
                appGroup.CustomVariableGroupIds.Add(groupLoadedFromSession.Id);
            }

            appGroup.CustomVariableGroupId = null;  // No longer need this property now that we have CustomVariableGroupIds (plural)
        }

        public void Save(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            applicationServer.ApplicationIdsForAllAppWithGroups         = new List<string>();
            applicationServer.CustomVariableGroupIdsForAllAppWithGroups = new List<string>();
            applicationServer.CustomVariableGroupIds                    = new List<string>();

            applicationServer.InstallationEnvironmentId = applicationServer.InstallationEnvironment.Id;

            // For each group, set its ApplicationId and CustomVariableGroupId.
            
            applicationServer.CustomVariableGroupIdsForGroupsWithinApps = new List<string>();

            foreach (ApplicationWithOverrideVariableGroup appGroup in applicationServer.ApplicationsWithOverrideGroup)
            {
                applicationServer.ApplicationIdsForAllAppWithGroups.Add(appGroup.Application.Id);
                appGroup.ApplicationId = appGroup.Application.Id;

                appGroup.CustomVariableGroupIds = new List<string>();  // Clear and add the IDs from the groups.
                if (appGroup.CustomVariableGroups != null)
                {
                    foreach (var group in appGroup.CustomVariableGroups)
                    {
                        applicationServer.CustomVariableGroupIdsForAllAppWithGroups.Add(group.Id);
                        appGroup.CustomVariableGroupIds.Add(group.Id);
                    }
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

        #region [ServerForceInstallation]

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IEnumerable<ServerForceInstallation> GetForceInstallationsByServerId(string serverId)
        {
            return ExecuteQuery<IEnumerable<ServerForceInstallation>>(() =>
            {
                IEnumerable<ServerForceInstallation> forceInstallations = QueryAndSetEtags(session =>
                    session.Query<ServerForceInstallation>()
                    .Include(x => x.ApplicationId)
                    .Include(x => x.ApplicationServerId)
                    .Include(x => x.OverrideGroupIds)
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.ApplicationServerId == serverId)
                    .Take(int.MaxValue)                    
                    ).AsEnumerable().Cast<ServerForceInstallation>();

                foreach (ServerForceInstallation serverForceInstallation in forceInstallations)
                {
                    serverForceInstallation.ApplicationServer =
                        QuerySingleResultAndSetEtag(session => session.Load<ApplicationServer>(serverForceInstallation.ApplicationServerId)) as ApplicationServer;

                    serverForceInstallation.ApplicationWithOverrideGroup = new ApplicationWithOverrideVariableGroup();

                    serverForceInstallation.ApplicationWithOverrideGroup.Application =
                        QuerySingleResultAndSetEtag(session => session.Load<Application>(serverForceInstallation.ApplicationId)) as Application;

                    if (serverForceInstallation.OverrideGroupIds == null || serverForceInstallation.OverrideGroupIds.Count < 1) { continue; }

                    serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();

                    if (serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroupIds == null)
                    {
                        serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroupIds = new List<string>();
                    }

                    foreach (var groupId in serverForceInstallation.OverrideGroupIds)
                    {
                        // ToDo: This is bad. We're breaking the concept of only using CustomVariableGroupIds in
                        //       the data layer. But we've already done so elsewhere. Adding the IDs here to see
                        //       if this will just cause things to start working.
                        if (!serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroupIds.Contains(groupId))
                        {
                            serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroupIds.Add(groupId);
                        }
                        serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroups.Add(
                            QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(groupId))
                            as CustomVariableGroup);
                    }
                }

                return forceInstallations;
            });
        }

        public void SaveForceInstallations(IEnumerable<ServerForceInstallation> serverForceInstallations)
        {
            if (serverForceInstallations == null) { throw new ArgumentNullException("serverForceInstallations"); }

            GenericData data = new GenericData();

            foreach (ServerForceInstallation serverForceInstallation in serverForceInstallations)
            {
                serverForceInstallation.ApplicationServerId = serverForceInstallation.ApplicationServer.Id;
                serverForceInstallation.ApplicationId       = serverForceInstallation.ApplicationWithOverrideGroup.Application.Id;

                if (serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroups != null
                 && serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroups.Count > 0)
                {
                    if (serverForceInstallation.OverrideGroupIds == null) { serverForceInstallation.OverrideGroupIds = new List<string>(); }

                    foreach (var cvg in serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroups)
                    {
                        serverForceInstallation.OverrideGroupIds.Add(cvg.Id);
                    }
                }

                data.Save(serverForceInstallation);
            }
        }

        public void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            new GenericData().Delete(forceInstallation);
        }

        #endregion
    }
}
