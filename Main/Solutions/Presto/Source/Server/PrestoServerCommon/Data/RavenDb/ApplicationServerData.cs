using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace PrestoServer.Data.RavenDb
{
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
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

                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIds))
                        .Customize(x => x.Include<ApplicationServer>(y => y.ApplicationIdsForAllAppWithGroups))
                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForAllAppWithGroups))
                        .Customize(x => x.Include<ApplicationServer>(y => y.CustomVariableGroupIdsForGroupsWithinApps))
                        .Customize(x => x.Include<ApplicationServer>(y => y.InstallationEnvironmentId))
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
                    .Where(server => server.Name == serverName).FirstOrDefault())
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
                    session
                    .Include<ApplicationServer>(x => x.CustomVariableGroupIds)
                    .Include<ApplicationServer>(x => x.ApplicationIdsForAllAppWithGroups)
                    .Include<ApplicationServer>(x => x.CustomVariableGroupIdsForAllAppWithGroups)
                    .Include<ApplicationServer>(x => x.CustomVariableGroupIdsForGroupsWithinApps)
                    .Include<ApplicationServer>(x => x.InstallationEnvironmentId)
                    .Load <ApplicationServer>(id))
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

            appServer.InstallationEnvironment = QuerySingleResultAndSetEtag(session =>
                session.Load<InstallationEnvironment>(appServer.InstallationEnvironmentId)) as InstallationEnvironment;
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

        #region [ServerForceInstallation]

        public IEnumerable<ServerForceInstallation> GetForceInstallationsByServerId(string serverId)
        {
            return ExecuteQuery<IEnumerable<ServerForceInstallation>>(() =>
            {
                IEnumerable<ServerForceInstallation> forceInstallations = QueryAndSetEtags(session =>
                    session.Query<ServerForceInstallation>()
                    .Include(x => x.ApplicationId)
                    .Include(x => x.ApplicationServerId)
                    .Include(x => x.OverrideGroupId)
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

                    if (serverForceInstallation.OverrideGroupId == null) { continue; }

                    serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroup =
                        QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(serverForceInstallation.OverrideGroupId)) as CustomVariableGroup;
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

                if (serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroup != null)
                {
                    serverForceInstallation.OverrideGroupId = serverForceInstallation.ApplicationWithOverrideGroup.CustomVariableGroup.Id;
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
