using System;
using System.Globalization;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Logic;

namespace PrestoCommon.Misc
{
    public class AppInstaller : IAppInstaller
    {
        public void InstallApplication(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }

            InstallationSummary installationSummary = new InstallationSummary(appWithGroup, server, DateTime.Now);

            InstallationResultContainer resultContainer = appWithGroup.Install(server);

            installationSummary.SetResults(resultContainer, DateTime.Now);

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
