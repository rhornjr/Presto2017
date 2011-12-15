using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using PrestoCommon.Enums;
using PrestoCommon.Logic;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// ApplicationServer entity
    /// </summary>
    public class ApplicationServer
    {
        private ObservableCollection<Application> _applications;
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

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
        public ObservableCollection<Application> Applications
        {
            get
            {
                if (this._applications == null) { this._applications = new ObservableCollection<Application>(); }

                return this._applications;
            }
        }

        /// <summary>
        /// Gets or sets the application to force install.
        /// </summary>
        /// <value>
        /// The application to force install.
        /// </value>
        public Application ApplicationToForceInstall { get; set; }

        /// <summary>
        /// Gets the custom variable groups.
        /// </summary>
        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                if (this._customVariableGroups == null) { this._customVariableGroups = new ObservableCollection<CustomVariableGroup>(); }

                return this._customVariableGroups;
            }
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
            // Get the list of InstallationStatus entities to validate against our list of apps.                
            IEnumerable<InstallationSummary> installationSummaryList = InstallationSummaryLogic.GetByServerName(this.Name);

            // If we find an app that needs to be installed, install it.
            foreach (Application app in this.Applications)
            {
                if (ApplicationShouldBeInstalled(app, installationSummaryList))
                {
                    LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture, PrestoCommonResources.AppWillBeInstalledOnAppServer, app.Name, this.Name));

                    InstallApplication(app);
                }
            }
        }

        private bool ApplicationShouldBeInstalled(Application application, IEnumerable<InstallationSummary> installationSummaryList)
        {
            // ToDo: Log all these decisions for debugging.

            // First, if this app has never been installed, then it needs to be.
            if (installationSummaryList == null || installationSummaryList.Count() < 1) { return true; }

            if (this.ApplicationToForceInstall != null && this.ApplicationToForceInstall.Name == application.Name)
            {
                this.ApplicationToForceInstall = null;  // Remove the app as force installing so we don't keep repeatedly installing it.
                LogicBase.Save<ApplicationServer>(this);
                return true;
            }

            IEnumerable<InstallationSummary> appSpecificInstallationSummaryList = installationSummaryList.Where(summary => summary.Application == application);

            if (appSpecificInstallationSummaryList == null || appSpecificInstallationSummaryList.Count() < 1) { return true; }

            // If there is no force installation time, then no need to install.
            if (application.ForceInstallation == null || application.ForceInstallation.ForceInstallationTime == null) { return false; }

            // Check the latest installation. If it's before ForceInstallationTime, then we need to install
            InstallationSummary mostRecentInstallationSummary = appSpecificInstallationSummaryList.OrderByDescending(summary => summary.InstallationStart).FirstOrDefault();

            if (mostRecentInstallationSummary.InstallationStart < application.ForceInstallation.ForceInstallationTime &&
                application.ForceInstallation.ForceInstallationEnvironment == this.DeploymentEnvironment) { return true; }

            return false;
        }

        private static bool InstallationSummaryFoundForApplication(IEnumerable<InstallationSummary> installationSummaryList, Application app)
        {           
            return installationSummaryList.Where(summary => summary.Application.Name == app.Name).FirstOrDefault() != null;
        }

        private void InstallApplication(Application application)
        {
            InstallationSummary installationSummary = new InstallationSummary(application, this, DateTime.Now);

            installationSummary.InstallationResult = application.Install(this);

            installationSummary.InstallationEnd = DateTime.Now;

            LogAndSaveInstallationSummary(installationSummary);
        }

        private static void LogAndSaveInstallationSummary(InstallationSummary installationSummary)
        {
            LogicBase.Save<InstallationSummary>(installationSummary);

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                PrestoCommonResources.ApplicationInstalled,
                installationSummary.Application.Name,
                installationSummary.ApplicationServer.Name,
                installationSummary.InstallationStart.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationEnd.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationResult.ToString()));
        }
    }
}
