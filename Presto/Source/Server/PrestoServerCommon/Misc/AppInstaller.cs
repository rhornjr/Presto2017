using System;
using System.Globalization;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Xanico.Core;

namespace PrestoServer.Misc
{
    public class AppInstaller : IAppInstaller
    {
        public void InstallApplication(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }

            DateTime installationStartTime = DateTime.Now;

            var installationSummary = new InstallationSummary(appWithGroup, server, installationStartTime);

            InstallationResultContainer resultContainer = appWithGroup.Install(server, installationStartTime);

            installationSummary.SetResults(resultContainer, DateTime.Now);

            LogAndSaveInstallationSummary(installationSummary);
        }

        private static void LogAndSaveInstallationSummary(InstallationSummary installationSummary)
        {
            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                prestoWcf.Service.SaveInstallationSummary(installationSummary);
            }

            Logger.LogInformation(string.Format(CultureInfo.CurrentCulture,
                PrestoServerResources.ApplicationInstalled,
                installationSummary.ApplicationWithOverrideVariableGroup.ToString(),
                installationSummary.ApplicationServer.Name,
                installationSummary.InstallationStart.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationEnd.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationResult.ToString()));
        }
    }
}
