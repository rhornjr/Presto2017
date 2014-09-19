﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Raven.Abstractions.Extensions;
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

            HydrateTaskApps(appWithGroup);

            InstallationResultContainer resultContainer = appWithGroup.Install(server, installationStartTime, true);

            installationSummary.SetResults(resultContainer, DateTime.Now);

            LogAndSaveInstallationSummary(installationSummary);
        }

        private static void HydrateTaskApps(ApplicationWithOverrideVariableGroup appWithGroupBundle)
        {
            // TaskApp tasks are stored with only the IDs for the Application and CustomVariableGroup properties.
            // Hydrate those before installing.

            foreach (var task in appWithGroupBundle.Application.Tasks.Where(x => x.PrestoTaskType == TaskType.App))
            {
                var taskApp = task as TaskApp;

                if (taskApp.AppWithGroup.Application == null)
                {
                    using (var prestoWcf = new PrestoWcf<IApplicationService>())
                    {
                        taskApp.AppWithGroup.Application = prestoWcf.Service.GetById(taskApp.AppWithGroup.ApplicationId);
                    }
                }

                if (taskApp.AppWithGroup.CustomVariableGroup == null &&
                    !string.IsNullOrWhiteSpace(taskApp.AppWithGroup.CustomVariableGroupId))
                {
                    using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                    {
                        taskApp.AppWithGroup.CustomVariableGroup =
                            prestoWcf.Service.GetById(taskApp.AppWithGroup.CustomVariableGroupId);
                    }
                }

                if (taskApp.AppWithGroup.CustomVariableGroup == null)
                {
                    taskApp.AppWithGroup.CustomVariableGroup = new CustomVariableGroup();
                    taskApp.AppWithGroup.CustomVariableGroup.CustomVariables = new ObservableCollection<CustomVariable>();
                }

                // Add the custom variables of each of the bundle's groups to the group of the taskApp.
                appWithGroupBundle.Application.CustomVariableGroups.ForEach(x =>
                    taskApp.AppWithGroup.CustomVariableGroup.CustomVariables.AddRange(x.CustomVariables));
            }
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
