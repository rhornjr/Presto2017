using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Enums;
using PrestoServer.Logic;

namespace PrestoAutomatedTests
{
    internal static class TestHelper
    {
        private static InstallationEnvironment _defaultEnv;
        private static List<Application> _genericApps = new List<Application>();

        static TestHelper()
        {
            TestUtility.PossiblyAddInstallationEnvironments();
            _defaultEnv = InstallationEnvironmentLogic.GetAll().First();

            // Create some generic apps so our servers can have more than one app assigned to them.
            for (int i = 1; i <= 5; i++)
            {
                var app = CreateApp("Generic" + i);
                _genericApps.Add(app);
            }
        }

        internal static void CreateAndPersistEntitiesForAUseCase(string rootName, int numberOfInstallationSummariesToCreate)
        {
            var app    = CreateApp(rootName);
            var group  = CreateCustomVariableGroup(rootName);
            var server = CreateAppServer(rootName, app, group);
            CreateInstallationSummaries(app, server, numberOfInstallationSummariesToCreate);
        }

        // internal so the test methods can call this as well
        internal static string GetAppName(string rootName)
        {
            return rootName + " app";
        }

        internal static string GetServerName(string rootName)
        {
            return rootName + " server";
        }

        internal static Application CreateApp(string rootName)
        {
            Application app = new Application();

            app.Name = rootName + " app";
            app.Version = "1.0.0.0";

            app.Tasks.Add(new TaskDosCommand("Just exit " + rootName, 1, 1, false, "cmd", "/c exit"));

            ApplicationLogic.Save(app);

            return app;
        }

        internal static CustomVariableGroup CreateCustomVariableGroup(string rootName)
        {
            CustomVariableGroup group = new CustomVariableGroup();

            group.Name = rootName + " group";

            group.CustomVariables.Add(new CustomVariable() { Key = rootName + " key", Value = rootName + " value" });

            CustomVariableGroupLogic.Save(group);

            return group;
        }

        private static ApplicationServer CreateAppServer(string rootName, Application app, CustomVariableGroup group)
        {
            ApplicationServer server = new ApplicationServer();

            server.Name                    = rootName + " server";
            server.InstallationEnvironment = _defaultEnv;
            server.Description             = rootName + " server description";
            server.EnableDebugLogging      = false;

            server.ApplicationsWithOverrideGroup.Add(new ApplicationWithOverrideVariableGroup() { Enabled = true, Application = app } );

            // Add the generic apps to the server, so we have more than just the one above.
            _genericApps.ForEach(x => server.ApplicationsWithOverrideGroup.Add(
                new ApplicationWithOverrideVariableGroup() { Enabled = true, Application = x } ));

            server.CustomVariableGroups.Add(group);

            ApplicationServerLogic.Save(server);

            return server;
        }

        private static void CreateInstallationSummaries(Application app, ApplicationServer server, int numberOfInstallationSummariesToCreate)
        {
            DateTime originalStartTime = DateTime.Now.AddDays(-1);

            for (int x = 0; x < numberOfInstallationSummariesToCreate; x++)
            {
                var appWithGroup = new ApplicationWithOverrideVariableGroup() { Application = app, ApplicationId = app.Id };
                DateTime startTime = originalStartTime.AddMinutes(x);

                InstallationSummary summary = new InstallationSummary(appWithGroup, server, startTime);

                var resultContainer = new InstallationResultContainer();
                resultContainer.InstallationResult = InstallationResult.Success;

                summary.SetResults(resultContainer, startTime.AddSeconds(4));

                InstallationSummaryLogic.Save(summary);
            }
        }
    }
}
