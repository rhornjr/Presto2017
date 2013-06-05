using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Microsoft.Practices.Unity;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;
using Xanico.Core;

namespace PrestoServer.Logic
{
    public static class ApplicationServerLogic
    {
        public static IEnumerable<ApplicationServer> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetAll();
        }

        public static ApplicationServer GetByName(string serverName)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetByName(serverName);
        }

        public static ApplicationServer GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetById(id);
        }

        public static void Save(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            try
            {
                DataAccessFactory.GetDataInterface<IApplicationServerData>().Save(applicationServer);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, applicationServer.Name);
                throw;
            }
        }

        #region [ServerForceInstallation]

        public static IEnumerable<ServerForceInstallation> GetForceInstallationsByServerId(string serverId)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetForceInstallationsByServerId(serverId);
        }

        public static void SaveForceInstallation(ServerForceInstallation serverForceInstallation)
        {
            List<ServerForceInstallation> serverForceInstallations = new List<ServerForceInstallation>();

            serverForceInstallations.Add(serverForceInstallation);

            SaveForceInstallations(serverForceInstallations);
        }

        public static void SaveForceInstallations(IEnumerable<ServerForceInstallation> serverForceInstallations)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().SaveForceInstallations(serverForceInstallations);
        }

        public static void RemoveForceInstallation(ServerForceInstallation forceInstallation)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().RemoveForceInstallation(forceInstallation);
        }

        #endregion

        #region [Installation Methods]

        public static void InstallApplications(ApplicationServer appServer)
        {
            if (appServer == null) { throw new ArgumentNullException("appServer"); }

            // If we find an app that needs to be installed, install it.
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in appServer.ApplicationsWithOverrideGroup)
            {
                bool shouldInstall = ApplicationShouldBeInstalled(appServer, appWithGroup);

                RemoveForceInstallation(appServer, appWithGroup);

                if (!shouldInstall) { continue; }

                Logger.LogInformation(string.Format(CultureInfo.CurrentCulture,
                    PrestoServerResources.AppWillBeInstalledOnAppServer,
                    appWithGroup,
                    appServer.Name));

                PrestoServerUtility.Container.Resolve<IAppInstaller>().InstallApplication(appServer, appWithGroup);
            }
        }

        private static void RemoveForceInstallation(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // If this was a force install, remove it, so we don't keep trying to install it repeatedly.
            ServerForceInstallation forceInstallation = appServer.GetFromForceInstallList(appWithGroup);

            if (forceInstallation != null)
            {
                appServer.ForceInstallationsToDo.Remove(forceInstallation);  // Remove so we don't install again.
                ApplicationServerLogic.RemoveForceInstallation(forceInstallation);
            }
        }

        private static bool FinalInstallationChecksPass(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            GlobalSetting globalSetting = GlobalSettingLogic.GetItem();

            string warning = string.Empty;

            if (globalSetting == null)
            {
                warning = string.Format(CultureInfo.CurrentCulture,
                    PrestoServerResources.GlobalSettingNullSoNoInstallation,
                    appWithGroup.ToString(),
                    appServer.Name);
                Logger.LogWarning(warning);
                LogMessageLogic.SaveLogMessage(warning);
                return false;
            }

            if (globalSetting.FreezeAllInstallations)
            {
                warning = string.Format(CultureInfo.CurrentCulture,
                    PrestoServerResources.FreezeAllInstallationsTrueSoNoInstallation,
                    appWithGroup.ToString(),
                    appServer.Name);
                Logger.LogWarning(warning);
                LogMessageLogic.SaveLogMessage(warning);
                return false;
            }

            return true;
        }

        internal static bool ApplicationShouldBeInstalled(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (IndividualChecksPass(appServer, appWithGroup) == false) { return false; }

            // This could be the first check in this method because if the freeze is set, it doesn't matter what the other conditions are.
            // However, we want this last, so we can view the logs for what would have happened even if the freeze was set.
            if (FinalInstallationChecksPass(appServer, appWithGroup)) { return true; }

            return false;
        }

        private static bool IndividualChecksPass(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (AppGroupEnabled(appServer, appWithGroup) == false) { return false; }

            // This is forcing an APPLICATION SERVER / APP GROUP instance
            if (ForceInstallIsThisAppWithGroup(appServer, appWithGroup)) { return true; }

            // This is forcing an APPLICATION
            // If there is no force installation time, then no need to install.
            if (!ForceInstallationExists(appServer, appWithGroup)) { return false; }
            if (ForceInstallationShouldHappenBasedOnTimeAndEnvironment(appServer, appWithGroup)) { return true; }

            return false;
        }

        private static bool AppGroupEnabled(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            Logger.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. ApplicationWithOverrideVariableGroup enabled: {0}.",
                appWithGroup.Enabled), appServer.EnableDebugLogging);

            return appWithGroup.Enabled;
        }

        private static bool ForceInstallIsThisAppWithGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            bool forceInstallIsThisAppWithGroup = false;  // default

            PossiblyRefreshForceInstallationsToDo(appServer);
            ServerForceInstallation forceInstall = appServer.GetFromForceInstallList(appWithGroup);

            if (forceInstall != null) { forceInstallIsThisAppWithGroup = true; }

            Logger.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. App server's ApplicationWithGroupToForceInstall matches the app group's " +
                "application in name and version: {0}. If this value is true, then the app will be installed. Note: If there " +
                "was a custom variable group, that matched by name as well. ApplicationServer.ApplicationWithGroupToForceInstall: {1}." +
                "^^^^ ApplicationWithOverrideVariableGroup: {2}.",
                forceInstallIsThisAppWithGroup,
                ForceInstallAsString(forceInstall),
                ApplicationWithGroupAsString(appWithGroup)), appServer.EnableDebugLogging);

            return forceInstallIsThisAppWithGroup;
        }

        // We don't persist these; this is just used in memory when determing what force installations to process.
        // Because of this, when the Presto Task Runner removes from ServerForceInstallations (completely separate
        // in the database from the ApplicationServer class), we won't get concurrency issues.
        private static void PossiblyRefreshForceInstallationsToDo(ApplicationServer appServer)
        {
            if (appServer.ForceInstallationsToDo != null && appServer.ForceInstallationsToDo.Count > 0) { return; }  // already have force installations ready to process
            
            appServer.ForceInstallationsToDo = new List<ServerForceInstallation>(ApplicationServerLogic.GetForceInstallationsByServerId(appServer.Id));
        }

        private static string ForceInstallAsString(ServerForceInstallation forceInstall)
        {
            if (forceInstall == null) { return string.Empty; }

            string groupName = string.Empty;

            if (forceInstall.ApplicationWithOverrideGroup.CustomVariableGroup != null) { groupName = forceInstall.ApplicationWithOverrideGroup.CustomVariableGroup.Name; }

            return string.Format(CultureInfo.CurrentCulture,
                "App name={0}, app version={1}, custom group name={2}",
                forceInstall.ApplicationWithOverrideGroup.Application.Name,
                forceInstall.ApplicationWithOverrideGroup.Application.Version,
                groupName);
        }

        private static string ApplicationWithGroupAsString(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup == null) { return string.Empty; }

            string groupName = string.Empty;

            if (appWithGroup.CustomVariableGroup != null) { groupName = appWithGroup.CustomVariableGroup.Name; }

            return string.Format(CultureInfo.CurrentCulture,
                "App name={0}, app version={1}, custom group name={2}",
                appWithGroup.Application.Name,
                appWithGroup.Application.Version,
                groupName);
        }

        private static bool ForceInstallationExists(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            bool forceInstallationExists =
                (appWithGroup.Application.ForceInstallation != null && appWithGroup.Application.ForceInstallation.ForceInstallationTime != null);

            Logger.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. ApplicationWithOverrideVariableGroup.Application.ForceInstallation exists: {0}. " +
                "ForceInstallationTime={1}",
                forceInstallationExists,
                ForceInstallationTimeAsString(appWithGroup)), appServer.EnableDebugLogging);

            return forceInstallationExists;
        }

        private static string ForceInstallationTimeAsString(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup.Application == null ||
                appWithGroup.Application.ForceInstallation == null ||
                appWithGroup.Application.ForceInstallation.ForceInstallationTime == null)
            {
                return "n/a";
            }

            return appWithGroup.Application.ForceInstallation.ForceInstallationTime.ToString();
        }

        private static bool ForceInstallationShouldHappenBasedOnTimeAndEnvironment(
            ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // Note: To even get to this method, a force installation exists.

            DateTime now = DateTime.Now;

            // Get the list of InstallationStatus entities for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithGroup);

            bool shouldForce;

            if (mostRecentInstallationSummary == null)
            {
                shouldForce = now > appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                    appWithGroup.Application.ForceInstallation.ForceInstallEnvironment.Id == appServer.InstallationEnvironment.Id;
                LogForceInstallExistsWithNoInstallationSummaries(appServer, appWithGroup, now, shouldForce);
                return shouldForce;
            }

            // Check the latest installation. If it's before ForceInstallationTime, then we need to install            
            shouldForce = (mostRecentInstallationSummary.InstallationStart < appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                now > appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                appWithGroup.Application.ForceInstallation.ForceInstallEnvironment.Id == appServer.InstallationEnvironment.Id);

            LogForceInstallBasedOnInstallationSummary(appServer, appWithGroup, now, mostRecentInstallationSummary, shouldForce);

            return shouldForce;
        }

        private static void LogForceInstallExistsWithNoInstallationSummaries(
            ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup, DateTime now, bool shouldForce)
        {
            Logger.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "{0} should be installed on {1}: {2}. A force installation exists, there are no installation summaries, " +
                "**AND** now ({3}) > appWithGroup.Application.ForceInstallation.ForceInstallationTime ({4}) " +
                "**AND** appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment ({5}) == this.DeploymentEnvironment ({6}).",
                appWithGroup.ToString(),
                appServer.Name,
                shouldForce,
                now.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallationTime.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallEnvironment,
                appServer.InstallationEnvironment),
                appServer.EnableDebugLogging);
        }

        private static void LogForceInstallBasedOnInstallationSummary(ApplicationServer appServer,
            ApplicationWithOverrideVariableGroup appWithGroup, DateTime now, InstallationSummary mostRecentInstallationSummary, bool shouldForce)
        {
            Logger.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. Force installation should happen: {0}. If true, the following is true: " +
                "mostRecentInstallationSummary.InstallationStart ({1}) < appWithGroup.Application.ForceInstallation.ForceInstallationTime ({2}) " +
                "**AND** now ({3}) > appWithGroup.Application.ForceInstallation.ForceInstallationTime ({4}) " +
                "**AND** appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment ({5}) == this.DeploymentEnvironment ({6}).",
                shouldForce,
                mostRecentInstallationSummary.InstallationStart.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallationTime.ToString(),
                now.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallationTime.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallEnvironment,
                appServer.InstallationEnvironment),
                appServer.EnableDebugLogging);
        }

        public static void InstallPrestoSelfUpdater(ApplicationServer appServer)
        {
            if (appServer == null) { throw new ArgumentNullException("appServer"); }

            string selfUpdatingAppName = ConfigurationManager.AppSettings["selfUpdatingAppName"];

            // Get the self-updater app from the DB
            //var appWithGroup =
            //    appServer.ApplicationsWithOverrideGroup.Where(x => x.Application.Name == selfUpdatingAppName).FirstOrDefault();

            var appWithGroup = new ApplicationWithOverrideVariableGroup();
            var app = ApplicationLogic.GetByName(selfUpdatingAppName);
            appWithGroup.Application = app;

            if (app == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, PrestoServerResources.PrestoSelfUpdaterAppNotFound, selfUpdatingAppName);
                throw new InvalidOperationException(message);
            }

            PrestoServerUtility.Container.Resolve<IAppInstaller>().InstallApplication(appServer, appWithGroup);
        }

        #endregion
    }
}
