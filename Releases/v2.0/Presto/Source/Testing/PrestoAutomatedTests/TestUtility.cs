using System;
using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Logic;

namespace PrestoAutomatedTests
{
    // The way this all works, at least for RavenDB at the moment:
    // 1. Start with a fresh DB - Delete C:\Software\RavenDb\RavenDB-Build-573\Server\Data
    // 2. Start the DB server: C:\Software\RavenDb\RavenDB-Build-573\Server\Raven.Server.exe
    // 3. Run the tests (CTRL-R-A)

    public static class TestUtility
    {
        private static bool _dataPopulated;

        public static readonly int TotalNumberOfEachEntityToCreate = 10;
        public static readonly int TotalNumberOfInstallationSummaries = 250;
        public static readonly int TotalNumberOfLogMessages = 1000;

        public static List<InstallationSummary> AllInstallationSummaries { get; private set; }

        public static void PopulateData()
        {
            if (_dataPopulated) { return; }

            AddApplications();
            AddCustomVariableGroups();
            AddAppServers();
            AddInstallationSummaries();
            AddLogMessages();

            _dataPopulated = true;
        }        

        private static void AddApplications()
        {
            for (int i = 1; i <= TotalNumberOfEachEntityToCreate; i++)
            {
                Application app = new Application();

                app.Name = "app" + i;
                app.Version = "1.0.0." + i;

                app.Tasks.Add(new TaskDosCommand("Just exit " + i, 1, 1, false, "cmd", "/c exit"));

                ApplicationLogic.Save(app);
            }
        }

        private static void AddCustomVariableGroups()
        {
            for (int i = 1; i <= TotalNumberOfEachEntityToCreate; i++)
            {
                CustomVariableGroup group = new CustomVariableGroup();

                group.Name = "group" + i;

                // For each group, add some custom variables. The first group will have one variable,
                // the second will have two, and so on...
                for (int x = 1; x <= i; x++)
                {
                    group.CustomVariables.Add(new CustomVariable() { Key = "k" + x, Value = "v" + x });
                }

                CustomVariableGroupLogic.Save(group);
            }
        }

        private static void AddAppServers()
        {
            for (int i = 1; i <= TotalNumberOfEachEntityToCreate; i++)
            {
                ApplicationServer server = new ApplicationServer();

                server.Name                          = "server" + i;
                server.DeploymentEnvironment         = DeploymentEnvironment.Development;
                server.Description                   = "Description " + i;
                server.EnableDebugLogging            = false;

                Application app = ApplicationLogic.GetByName("app" + i);
                server.ApplicationsWithOverrideGroup.Add(new ApplicationWithOverrideVariableGroup() { Enabled = true, Application = app } );

                CustomVariableGroup group   = CustomVariableGroupLogic.GetByName("group" + i);
                server.CustomVariableGroups.Add(group);

                ApplicationServerLogic.Save(server);
            }
        }

        private static void AddInstallationSummaries()
        {
            AllInstallationSummaries = new List<InstallationSummary>();

            List<Application> allApps          = new List<Application>(ApplicationLogic.GetAll());
            List<ApplicationServer> allServers = new List<ApplicationServer>(ApplicationServerLogic.GetAll());

            ApplicationWithOverrideVariableGroup appWithGroup;
            DateTime originalStartTime = DateTime.Now.AddDays(-1);

            int totalOuterLoops = TotalNumberOfInstallationSummaries / TotalNumberOfEachEntityToCreate;

            int runningTotal = 1;  // Count of total summaries overall
            for (int i = 1; i <= totalOuterLoops; i++)
            {
                for (int x = 0; x < TotalNumberOfEachEntityToCreate - 1; x++)  // We use "- 1" here so we have some entities without an installation summary (for testing)
                {
                    appWithGroup = new ApplicationWithOverrideVariableGroup() { Application = allApps[x], ApplicationId = allApps[x].Id };
                    DateTime startTime = originalStartTime.AddMinutes(runningTotal);

                    InstallationSummary summary = new InstallationSummary(appWithGroup, allServers[x], startTime);

                    summary.InstallationEnd = startTime.AddSeconds(4);
                    summary.InstallationResult = InstallationResult.Success;

                    AllInstallationSummaries.Add(summary);
                    InstallationSummaryLogic.Save(summary);

                    runningTotal++;
                }
            }
        }

        private static void AddLogMessages()
        {
            for (int i = 1; i <= TotalNumberOfLogMessages; i++)
            {
                LogMessageLogic.SaveLogMessage("Message " + i);
            }
        }        
    }
}
