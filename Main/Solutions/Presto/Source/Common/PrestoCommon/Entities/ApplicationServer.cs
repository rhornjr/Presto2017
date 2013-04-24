﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Practices.Unity;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class ApplicationServer : EntityBase
    {
        private string _name;
        private string _description;
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _applicationsWithOverrideGroup;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private List<ServerForceInstallation> _forceInstallationsToDo;

        // We don't persist these; this is just used in memory when determing what force installations to process.
        // Because of this, when the Presto Task Runner removes from ServerForceInstallations (completely separate
        // in the database from the ApplicationServer class), we won't get concurrency issues.
        [JsonIgnore]
        private List<ServerForceInstallation> ForceInstallationsToDo
        {
            get
            {
                if (this._forceInstallationsToDo == null)
                {
                    this._forceInstallationsToDo = new List<ServerForceInstallation>(ApplicationServerLogic.GetForceInstallationsByServerId(this.Id));
                }
                return this._forceInstallationsToDo;
            }
        }

        [DataMember]
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        [DataMember]
        public string Description
        {
            get { return this._description; }

            set
            {
                this._description = value;
                NotifyPropertyChanged(() => this.Description);
            }
        }

        [DataMember]
        public bool EnableDebugLogging { get; set; }

        [DataMember]
        public string InstallationEnvironmentId { get; set; }

        // ToDo: Remove this after migrating the data to InstallationEnvironment
        [DataMember]
        public DeploymentEnvironment DeploymentEnvironment { get; set; }

        [JsonIgnore]
        public InstallationEnvironment InstallationEnvironment { get; set; }

        [DataMember]
        public ObservableCollection<ApplicationWithOverrideVariableGroup> ApplicationsWithOverrideGroup
        {
            get
            {
                if (this._applicationsWithOverrideGroup == null) { this._applicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>(); }

                return this._applicationsWithOverrideGroup;
            }

            set { this._applicationsWithOverrideGroup = value; }
        }

        [DataMember]
        public List<string> ApplicationIdsForAllAppWithGroups { get; set; }  // For RavenDB

        [DataMember]
        public List<string> CustomVariableGroupIdsForAllAppWithGroups { get; set; }  // For RavenDB

        [DataMember]
        public List<string> CustomVariableGroupIdsForGroupsWithinApps { get; set; }  // For RavenDB        

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                if (this._customVariableGroups == null) { this._customVariableGroups = new ObservableCollection<CustomVariableGroup>(); }

                return this._customVariableGroups;
            }

            set { this._customVariableGroups = value; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Finds the matching <see cref="ApplicationWithOverrideVariableGroup"/> from the force install list.
        /// </summary>
        /// <param name="appWithGroup">The <see cref="ApplicationWithOverrideVariableGroup"/></param>
        /// <returns>If a match is found, returns the <see cref="ApplicationWithOverrideVariableGroup"/> instance.
        ///          If no match is found, returns null.
        /// </returns>
        public ServerForceInstallation GetFromForceInstallList(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            return CommonUtility.GetAppWithGroup(this.ForceInstallationsToDo, appWithGroup);
        }

        public void InstallApplications()
        {
            // If we find an app that needs to be installed, install it.
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in this.ApplicationsWithOverrideGroup)
            {
                bool shouldInstall = ApplicationShouldBeInstalled(appWithGroup);

                RemoveForceInstallation(appWithGroup);

                if (!shouldInstall) { continue; }

                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.AppWillBeInstalledOnAppServer, appWithGroup, this.Name));

                CommonUtility.Container.Resolve<IAppInstaller>().InstallApplication(this, appWithGroup);
            }                       
        }

        private void RemoveForceInstallation(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // If this was a force install, remove it, so we don't keep trying to install it repeatedly.
            ServerForceInstallation forceInstallation = this.GetFromForceInstallList(appWithGroup);

            if (forceInstallation != null)
            {
                this.ForceInstallationsToDo.Remove(forceInstallation);  // Remove so we don't install again.
                ApplicationServerLogic.RemoveForceInstallation(forceInstallation);
            }
        }

        private bool FinalInstallationChecksPass(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            GlobalSetting globalSetting = GlobalSettingLogic.GetItem();

            string warning = string.Empty;

            if (globalSetting == null)
            {
                warning = string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.GlobalSettingNullSoNoInstallation, appWithGroup.ToString(), this.Name);
                LogUtility.LogWarning(warning);
                LogMessageLogic.SaveLogMessage(warning);
                return false;
            }

            if (globalSetting.FreezeAllInstallations)
            {
                warning = string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.FreezeAllInstallationsTrueSoNoInstallation, appWithGroup.ToString(), this.Name);
                LogUtility.LogWarning(warning);
                LogMessageLogic.SaveLogMessage(warning);
                return false;
            }

            return true;
        }

        private bool ApplicationShouldBeInstalled(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (IndividualChecksPass(appWithGroup) == false) { return false; }

            // This could be the first check in this method because if the freeze is set, it doesn't matter what the other conditions are.
            // However, we want this last, so we can view the logs for what would have happened even if the freeze was set.
            if (FinalInstallationChecksPass(appWithGroup)) { return true; }

            return false;
        }

        private bool IndividualChecksPass(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (AppGroupEnabled(appWithGroup) == false) { return false; }

            // This is forcing an APPLICATION SERVER / APP GROUP instance
            if (ForceInstallIsThisAppWithGroup(appWithGroup)) { return true; }

            // This is forcing an APPLICATION
            // If there is no force installation time, then no need to install.
            if (!ForceInstallationExists(appWithGroup)) { return false; }
            if (ForceInstallationShouldHappenBasedOnTimeAndEnvironment(appWithGroup)) { return true; }

            return false;
        }

        private bool AppGroupEnabled(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. ApplicationWithOverrideVariableGroup enabled: {0}.",
                appWithGroup.Enabled), this.EnableDebugLogging);

            return appWithGroup.Enabled;
        }

        private bool ForceInstallIsThisAppWithGroup(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            bool forceInstallIsThisAppWithGroup = false;  // default

            ServerForceInstallation forceInstall = this.GetFromForceInstallList(appWithGroup);

            if (forceInstall != null) { forceInstallIsThisAppWithGroup = true; }            

            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. App server's ApplicationWithGroupToForceInstall matches the app group's " +
                "application in name and version: {0}. If this value is true, then the app will be installed. Note: If there " +
                "was a custom variable group, that matched by name as well. ApplicationServer.ApplicationWithGroupToForceInstall: {1}." +
                "^^^^ ApplicationWithOverrideVariableGroup: {2}.",
                forceInstallIsThisAppWithGroup,
                ForceInstallAsString(forceInstall),
                ApplicationWithGroupAsString(appWithGroup)), this.EnableDebugLogging);

            return forceInstallIsThisAppWithGroup;
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

        private bool ForceInstallationExists(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            bool forceInstallationExists =
                (appWithGroup.Application.ForceInstallation != null && appWithGroup.Application.ForceInstallation.ForceInstallationTime != null);

            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. ApplicationWithOverrideVariableGroup.Application.ForceInstallation exists: {0}. " +
                "ForceInstallationTime={1}",
                forceInstallationExists,
                ForceInstallationTimeAsString(appWithGroup)), this.EnableDebugLogging);

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

        private bool ForceInstallationShouldHappenBasedOnTimeAndEnvironment(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // Note: To even get to this method, a force installation exists.

            DateTime now = DateTime.Now;

            // Get the list of InstallationStatus entities for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(this, appWithGroup);

            bool shouldForce;

            if (mostRecentInstallationSummary == null)
            {
                shouldForce = now > appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                    appWithGroup.Application.ForceInstallation.ForceInstallEnvironment.Id == this.InstallationEnvironment.Id;
                LogForceInstallExistsWithNoInstallationSummaries(appWithGroup, now, shouldForce);
                return shouldForce;
            }

            // Check the latest installation. If it's before ForceInstallationTime, then we need to install            
            shouldForce = (mostRecentInstallationSummary.InstallationStart < appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                now > appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                appWithGroup.Application.ForceInstallation.ForceInstallEnvironment.Id == this.InstallationEnvironment.Id);

            LogForceInstallBasedOnInstallationSummary(appWithGroup, now, mostRecentInstallationSummary, shouldForce);

            return shouldForce;
        }

        private void LogForceInstallExistsWithNoInstallationSummaries(ApplicationWithOverrideVariableGroup appWithGroup, DateTime now, bool shouldForce)
        {
            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "{0} should be installed on {1}: {2}. A force installation exists, there are no installation summaries, " +
                "**AND** now ({3}) > appWithGroup.Application.ForceInstallation.ForceInstallationTime ({4}) " +
                "**AND** appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment ({5}) == this.DeploymentEnvironment ({6}).",
                appWithGroup.ToString(),
                this.Name,
                shouldForce,
                now.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallationTime.ToString(),
                appWithGroup.Application.ForceInstallation.ForceInstallEnvironment,
                this.InstallationEnvironment),
                    this.EnableDebugLogging);
        }

        private void LogForceInstallBasedOnInstallationSummary(ApplicationWithOverrideVariableGroup appWithGroup, DateTime now,
            InstallationSummary mostRecentInstallationSummary, bool shouldForce)
        {
            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
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
                this.InstallationEnvironment),
                    this.EnableDebugLogging);
        }

        public void InstallPrestoSelfUpdater()
        {
            string selfUpdatingAppName = ConfigurationManager.AppSettings["selfUpdatingAppName"];

            // Get the self-updater app from the DB
            ApplicationWithOverrideVariableGroup appWithGroup =
                this.ApplicationsWithOverrideGroup.Where(x => x.Application.Name == selfUpdatingAppName).FirstOrDefault();

            if (appWithGroup == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.PrestoSelfUpdaterAppNotFound, this.Name);
                throw new InvalidOperationException(message);
            }

            CommonUtility.Container.Resolve<IAppInstaller>().InstallApplication(this, appWithGroup);
        }
    }
}
