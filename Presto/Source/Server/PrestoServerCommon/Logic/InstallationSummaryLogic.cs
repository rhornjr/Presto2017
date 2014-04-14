﻿using System.Collections.Generic;
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
            DataAccessLayerBase.NewInstallationSummaryAddedToDb += OnInstallationSummaryAdded;
        }

        public static InstallationSummary GetMostRecentByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByServerAppAndGroup(appServer, appWithGroup);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTime(numberToRetrieve);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndServer(int numberToRetrieve, string serverId)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTimeAndServer(numberToRetrieve, serverId);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTimeAndApplication(int numberToRetrieve, string appId)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTimeAndApplication(numberToRetrieve, appId);
        }

        public static IEnumerable<InstallationSummary> GetMostRecentByStartTimeServerAndApplication(int numberToRetrieve, string serverId, string appId)
        {
            return DataAccessFactory.GetDataInterface<IInstallationSummaryData>().GetMostRecentByStartTimeServerAndApplication(numberToRetrieve, serverId, appId);
        }

        public static void Save(InstallationSummary installationSummary)
        {
            DataAccessFactory.GetDataInterface<IInstallationSummaryData>().Save(installationSummary);
        }

        private static void OnInstallationSummaryAdded(object sender, EventArgs<string> e)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<PrestoHub>();
            hubContext.Clients.All.OnInstallationSummaryAdded();
        }
    }
}
