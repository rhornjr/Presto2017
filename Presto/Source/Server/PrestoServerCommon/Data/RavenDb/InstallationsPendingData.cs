using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoServer.Data.Interfaces;
using Raven.Client;

namespace PrestoServer.Data.RavenDb
{
    public class InstallationsPendingData : DataAccessLayerBase, IInstallationsPendingData
    {
        public IEnumerable<ServerForceInstallation> GetPending()
        {
            return ExecuteQuery<IEnumerable<ServerForceInstallation>>(() =>
            {
                var pendingInstalls = QueryAndSetEtags(session =>
                    session.Query<ServerForceInstallation>()
                           .Include(x => x.ApplicationServerId)
                           .Include(x => x.ApplicationId)
                           .Include(x => x.OverrideGroupIds)
                           .Customize(x => x.WaitForNonStaleResults()))
                    .AsEnumerable().Cast<ServerForceInstallation>();

                if (pendingInstalls != null)
                {
                    foreach (var pending in pendingInstalls) { HydratePendingInstall(pending); }
                }

                return pendingInstalls;
            });
        }

        private static void HydratePendingInstall(ServerForceInstallation pendingInstall)
        {
            pendingInstall.ApplicationServer = QuerySingleResultAndSetEtag(
                session => session.Load<ApplicationServer>(pendingInstall.ApplicationServerId)) as ApplicationServer;

            pendingInstall.ApplicationWithOverrideGroup = new ApplicationWithOverrideVariableGroup();

            pendingInstall.ApplicationWithOverrideGroup.Application = QuerySingleResultAndSetEtag(
                session => session.Load<Application>(pendingInstall.ApplicationId)) as Application;

            pendingInstall.ApplicationWithOverrideGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();

            if (pendingInstall.OverrideGroupIds == null) { return; }
            foreach (string groupId in pendingInstall.OverrideGroupIds)
            {
                pendingInstall.ApplicationWithOverrideGroup.CustomVariableGroups.Add(QuerySingleResultAndSetEtag(
                    session => session.Load<CustomVariableGroup>(groupId)) as CustomVariableGroup);
            }
        }
    }
}
