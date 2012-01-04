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
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _applicationsWithOverrideGroup;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;

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
        /// Gets or sets the IP address.
        /// </summary>
        /// <value>
        /// The IP address.
        /// </value>
        public string IpAddress { get; set; }

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
        /// Gets or sets the application with group to force install.
        /// </summary>
        /// <value>
        /// The application with group to force install.
        /// </value>
        public ApplicationWithOverrideVariableGroup ApplicationWithGroupToForceInstall { get; set; }

        /// <summary>
        /// Gets or sets the custom variable group ids.
        /// </summary>
        /// <value>
        /// The custom variable group ids.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<string> CustomVariableGroupIds { get; set; }  // For RavenDB, grrrr...

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
        /// Installs the applications.
        /// </summary>
        public void InstallApplications()
        {            
            // If we find an app that needs to be installed, install it.
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in this.ApplicationsWithOverrideGroup)
            {
                if (!ApplicationShouldBeInstalled(appWithGroup)) { continue; }

                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.AppWillBeInstalledOnAppServer, appWithGroup.Application.Name, this.Name));
                InstallApplication(appWithGroup);
            }
        }

        private bool ApplicationShouldBeInstalled(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            // ToDo: Log all these decisions for debugging.

            if (appWithGroup.Enabled == false) { return false; }

            // Get the list of InstallationStatus entities to see if we've ever installed this app.
            IEnumerable<InstallationSummary> installationSummaryList = InstallationSummaryLogic.GetByServerNameAppVersionAndGroup(this.Name, appWithGroup);

            // First, if this app has never been installed, then it needs to be.
            if (installationSummaryList == null || installationSummaryList.Count() < 1) { return true; }

            if (ForceInstallIsThisAppWithGroup(appWithGroup))
            {
                this.ApplicationWithGroupToForceInstall = null;  // Remove the app as force installing so we don't keep repeatedly installing it.
                LogicBase.Save(this);
                return true;
            }

            // If there is no force installation time, then no need to install.
            if (appWithGroup.Application.ForceInstallation == null || appWithGroup.Application.ForceInstallation.ForceInstallationTime == null) { return false; }

            // Check the latest installation. If it's before ForceInstallationTime, then we need to install
            InstallationSummary mostRecentInstallationSummary = installationSummaryList.OrderByDescending(summary => summary.InstallationStart).FirstOrDefault();

            if (mostRecentInstallationSummary.InstallationStart < appWithGroup.Application.ForceInstallation.ForceInstallationTime &&
                appWithGroup.Application.ForceInstallation.ForceInstallationEnvironment == this.DeploymentEnvironment) { return true; }

            return false;
        }

        private bool ForceInstallIsThisAppWithGroup(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (this.ApplicationWithGroupToForceInstall != null && this.ApplicationWithGroupToForceInstall.Application.Name == appWithGroup.Application.Name &&
                this.ApplicationWithGroupToForceInstall.Application.Version == appWithGroup.Application.Version)
            {
                // If there is a custom variable group, those need to be equal as well.
                if ((this.ApplicationWithGroupToForceInstall.CustomVariableGroup == null && appWithGroup.CustomVariableGroup == null) ||
                    this.ApplicationWithGroupToForceInstall.CustomVariableGroup.Name == appWithGroup.CustomVariableGroup.Name)
                {
                    return true;
                }
            }

            return false;
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
            LogicBase.Save(installationSummary);

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
