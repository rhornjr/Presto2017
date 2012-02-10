using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using PrestoCommon.Enums;
using PrestoCommon.Logic;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// ApplicationServer entity
    /// </summary>
    public class ApplicationServer : EntityBase
    {
        private string _name;
        private string _description;
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _applicationsWithOverrideGroup;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private List<ApplicationWithOverrideVariableGroup> _applicationWithGroupToForceInstallList;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get { return this._description; }

            set
            {
                this._description = value;
                NotifyPropertyChanged(() => this.Description);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable debug logging].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable debug logging]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableDebugLogging { get; set; }

        /// <summary>
        /// Gets or sets the deployment environment.
        /// </summary>
        /// <value>
        /// The deployment environment.
        /// </value>
        public DeploymentEnvironment DeploymentEnvironment { get; set; }

        /// <summary>
        /// Gets the application.
        /// </summary>
        public ObservableCollection<ApplicationWithOverrideVariableGroup> ApplicationsWithOverrideGroup
        {
            get
            {
                if (this._applicationsWithOverrideGroup == null) { this._applicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>(); }

                return this._applicationsWithOverrideGroup;
            }

            set { this._applicationsWithOverrideGroup = value; }
        }

        /// <summary>
        /// Gets or sets the application ids.
        /// </summary>
        /// <value>
        /// The application ids.
        /// </value>
        public List<string> ApplicationIdsForAllAppWithGroups { get; set; }  // For RavenDB

        /// <summary>
        /// Gets or sets the custom variable group ids for all app with groups.
        /// </summary>
        /// <value>
        /// The custom variable group ids for all app with groups.
        /// </value>
        public List<string> CustomVariableGroupIdsForAllAppWithGroups { get; set; }  // For RavenDB

        /// <summary>
        /// Gets or sets the application with group to force install.
        /// </summary>
        /// <value>
        /// The application with group to force install.
        /// </value>
        public List<ApplicationWithOverrideVariableGroup> ApplicationWithGroupToForceInstallList
        {
            get
            {
                if (this._applicationWithGroupToForceInstallList == null)
                {
                    this._applicationWithGroupToForceInstallList = new List<ApplicationWithOverrideVariableGroup>();
                }
                return this._applicationWithGroupToForceInstallList;
            }

            set
            {
                this._applicationWithGroupToForceInstallList = value;
            }
        }

        /// <summary>
        /// Gets or sets the custom variable group ids.
        /// </summary>
        /// <value>
        /// The custom variable group ids.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB

        /// <summary>
        /// Gets the custom variable groups.
        /// </summary>
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
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
        public ApplicationWithOverrideVariableGroup GetFromForceInstallList(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            return this.ApplicationWithGroupToForceInstallList.Where(group =>
                group.ApplicationId == appWithGroup.ApplicationId &&
                ((group.CustomVariableGroupId == null && appWithGroup.CustomVariableGroupId == null) ||
                (group.CustomVariableGroupId == appWithGroup.CustomVariableGroupId))).FirstOrDefault();
        }

        /// <summary>
        /// Installs the applications.
        /// </summary>
        public void InstallApplications()
        {            
            // If we find an app that needs to be installed, install it.
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in this.ApplicationsWithOverrideGroup)
            {
                if (!ApplicationShouldBeInstalled(appWithGroup)) { continue; }
                
                // If this was a force install, remove it, so we don't keep trying to install it repeatedly.
                ApplicationWithOverrideVariableGroup forceInstallGroup = this.GetFromForceInstallList(appWithGroup);
                
                if (forceInstallGroup != null)
                {
                    this.ApplicationWithGroupToForceInstallList.Remove(forceInstallGroup);  // Remove so we don't install again.
                    ApplicationServerLogic.Save(this);
                }

                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.AppWillBeInstalledOnAppServer, appWithGroup, this.Name));
                InstallApplication(appWithGroup);
            }                       
        }

        private bool ApplicationShouldBeInstalled(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (AppGroupEnabled(appWithGroup) == false) { return false; }

            // Get the list of InstallationStatus entities to see if we've ever installed this app.
            IEnumerable<InstallationSummary> installationSummaryList = InstallationSummaryLogic.GetByServerAppAndGroup(this, appWithGroup);

            // First, if this app has never been installed, then it needs to be.
            if (!InstallationSummaryExists(installationSummaryList)) { return true; }

            InstallationSummary mostRecentInstallationSummary = installationSummaryList.OrderByDescending(summary => summary.InstallationStart).FirstOrDefault();

            //     ****  This is forcing an APPLICATION SERVER / APP GROUP instance ****

            if (ForceInstallIsThisAppWithGroup(appWithGroup)) { return true; }

            //     ****  This is forcing an APPLICATION  ****

            // If there is no force installation time, then no need to install.
            if (!ForceInstallationExists(appWithGroup)) { return false; }            

            if (ForceInstallationShouldHappenBasedOnTimeAndEnvironment(mostRecentInstallationSummary, appWithGroup)) { return true; }

            return false;
        }              

        private bool AppGroupEnabled(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. ApplicationWithOverrideVariableGroup enabled: {0}.",
                appWithGroup.Enabled), this.EnableDebugLogging);

            return appWithGroup.Enabled;
        }

        private bool InstallationSummaryExists(IEnumerable<InstallationSummary> installationSummaryList)
        {
            bool installationSummaryExists = (installationSummaryList != null && installationSummaryList.Count() > 0);

            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. InstallationSummary exists: {0}. If this value is false, then the app will be installed.",
                installationSummaryExists), this.EnableDebugLogging);

            return installationSummaryExists;
        }        

        private bool ForceInstallIsThisAppWithGroup(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            bool forceInstallIsThisAppWithGroup = false;  // default

            ApplicationWithOverrideVariableGroup forceInstallGroup = this.GetFromForceInstallList(appWithGroup);

            if (forceInstallGroup != null) { forceInstallIsThisAppWithGroup = true; }            

            LogUtility.LogDebug(string.Format(CultureInfo.CurrentCulture,
                "Checking if app should be installed. App server's ApplicationWithGroupToForceInstall matches the app group's " +
                "application in name and version: {0}. If this value is true, then the app will be installed. Note: If there " +
                "was a custom variable group, that matched by name as well. ApplicationServer.ApplicationWithGroupToForceInstall: {1}." +
                "^^^^ ApplicationWithOverrideVariableGroup: {2}.",
                forceInstallIsThisAppWithGroup,
                ApplicationWithGroupAsString(forceInstallGroup),
                ApplicationWithGroupAsString(appWithGroup)), this.EnableDebugLogging);

            return forceInstallIsThisAppWithGroup;
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

        private bool ForceInstallationShouldHappenBasedOnTimeAndEnvironment(InstallationSummary mostRecentInstallationSummary, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            DateTime now = DateTime.Now;

            // Check the latest installation. If it's before ForceInstallationTime, then we need to install            
            bool shouldForce = (mostRecentInstallationSummary.InstallationStart < appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                now > appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment == this.DeploymentEnvironment);

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
                appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment,
                this.DeploymentEnvironment),
                    this.EnableDebugLogging);

            return shouldForce;
        }  

        private void InstallApplication(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            InstallationSummary installationSummary = new InstallationSummary(appWithGroup, this, DateTime.Now);

            installationSummary.InstallationResult = appWithGroup.Install(this);

            installationSummary.InstallationEnd = DateTime.Now;

            LogAndSaveInstallationSummary(installationSummary);
        }

        private static void LogAndSaveInstallationSummary(InstallationSummary installationSummary)
        {
            InstallationSummaryLogic.Save(installationSummary);

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                PrestoCommonResources.ApplicationInstalled,
                installationSummary.ApplicationWithOverrideVariableGroup.ToString(),
                installationSummary.ApplicationServer.Name,
                installationSummary.InstallationStart.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationEnd.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationResult.ToString()));
        }
    }
}
