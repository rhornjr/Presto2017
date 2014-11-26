using System;
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
            // See ReadMe_AppWithGroupMapping.docx, in the same folder as this class, for a diagram overview.

            // When this method is complete, the innermost appWithGroup should have all of the CVGs in it,
            // because that is what gets passed to the tasks that execute. When a taskApp runs, it calls
            // this.AppWithGroup.Install(...). That means the tasks don't have access to the bundle; just
            // the appWithGroup within it.
            // The override CVGs will go here: innermostAppWithGroup.CVGs
            // All other CVGs will go here: innermostAppWithGroup.App.CVGs

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

                /***********************************************************************************************************
                 *                                               Normal CVGs                                               *
                 ***********************************************************************************************************/

                // Add the bundle's app's CVGs to the taskApp
                appWithGroupBundle.Application.CustomVariableGroups.ForEach(x => taskApp.AppWithGroup.Application.CustomVariableGroups.Add(x));

                // For each task app in the bundle, add its CVGs to the taskApp.
                // These need to be hydrated first.
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    foreach (var cvgId in taskApp.AppWithGroup.CustomVariableGroupIds)
                    {
                        var cvg = prestoWcf.Service.GetById(cvgId);
                        taskApp.AppWithGroup.Application.CustomVariableGroups.Add(cvg);
                    }
                }

                /***********************************************************************************************************
                 *                                                OVERRIDES                                                *
                 ***********************************************************************************************************/

                // It's not possible to have overrides on taskApp.AppWithGroup.CustomVariableGroups because the overrides
                // are set with the bundle. So let's initialize the CVGs so we can store the bundle's overrides here.
                taskApp.AppWithGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();

                // These are the overrides.
                if (appWithGroupBundle.CustomVariableGroups != null)
                {
                    taskApp.AppWithGroup.CustomVariableGroups.AddRange(appWithGroupBundle.CustomVariableGroups);
                }
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
                installationSummary.InstallationResult));
        }
    }
}

/********************************************************************************************************************
 * 
 * HydrateTaskApps() overview of objects with a test bundle, test app, and test overrides:
 * 
 * - appWithGroupBundle
 *   - app: zTestBundle
 *     - Tasks: zTest1 (taskApp)
 *               - appWithGroup
 *                 - app: zTest1
 *                   - CVGs: zTest1
 *                 - CVGs: null*
 *     - CVGs*: zTestBundle (gets copied to appWithGroup.app.CVGs above)
 *   - CVGs*: UnitPBG12 and zTest2 (The overrides. These get copied to the null CVGs above.)
 *   
 * Note: A server also has CVGs. The server is sent as a separate parameter to resolve CVGs.
 * 
 ********************************************************************************************************************/
