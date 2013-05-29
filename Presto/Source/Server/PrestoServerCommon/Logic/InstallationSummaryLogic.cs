using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using PrestoCommon.Entities;
using PrestoCommon.Misc;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using PrestoServer.Data.RavenDb;
using PrestoServer.SignalR;

namespace PrestoServer.Logic
{
    public static class InstallationSummaryLogic
    {
        static InstallationSummaryLogic()
        {
            // When there is a new installation summary, automatically refresh the list.
            DataAccessLayerBase.NewInstallationSummaryAddedToDb += OnDatabaseItemAdded;
        }

        public static InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByServerAppAndGroup(appServer, appWithGroup);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTime(numberToRetrieve);
        }

        public static void Save(InstallationSummary installationSummary)
        {
            DataAccessFactory.GetDataInterface<IInstallationSummaryData>().Save(installationSummary);
        }

        private static void OnDatabaseItemAdded(object sender, EventArgs<string> e)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<PrestoHub>();
            hubContext.Clients.All.OnDatabaseItemAdded("snuh");
        }
    }
}
