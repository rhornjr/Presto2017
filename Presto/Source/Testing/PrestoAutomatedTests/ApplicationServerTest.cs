using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoServer;
using PrestoServer.Logic;

namespace PrestoAutomatedTests
{
    /// <summary>
    ///This is a test class for ApplicationServerTest and is intended
    ///to contain all ApplicationServerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationServerTest
    {
        // ToDo: It is possible for the tests to pass for the wrong reasons. For example, if we set up a test to fail
        //       because we never saved/added a force installation, it's possible the install wouldn't happen because
        //       the appWithGroup is not enabled. So, it would be cool to know the actual reason the installation
        //       didn't happen, then we could confirm that it didn't install for the right reason.

        private TestContext _testContextInstance;
        private const bool _enableAppServerDebugLogging = true;
        private readonly Mock<IAppInstaller> _mockAppInstaller = RegisterMockAppInstaller();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            // Only one of these two lines should be active for a test run. If you don't want to
            // wait for data to populate, then only regesiter the Raven data classes.
            //CommonUtility.RegisterRavenDataClasses();
            TestUtility.PopulateData();
        }
        
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ApplicationShouldBeInstalled
        ///</summary>
        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase1()
        {
            // Use Case #1 -- app group is not enabled

            ApplicationServer appServer = GetAppServerWithInstallationSummariesFromDb();
            
            // If disabled, don't install.
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithNullGroup.Enabled = false;

            ApplicationServerLogic.SaveForceInstallation(new ServerForceInstallation(appServer, appWithNullGroup));

            SetGlobalFreeze(false);

            ApplicationServerLogic.InstallApplications(appServer);

            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Never());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase2()
        {
            // Use Case #2 -- app group exists in the server's ApplicationWithGroupToForceInstallList
            //             -- with *null* custom variable group

            ApplicationServer appServer = GetAppServerWithInstallationSummariesFromDb();

            // Use this app
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];

            ApplicationServerLogic.SaveForceInstallation(new ServerForceInstallation(appServer, appWithNullGroup));

            SetGlobalFreeze(false);

            ApplicationServerLogic.InstallApplications(appServer);

            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Once());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase3()
        {
            // Use Case #3 -- app group exists in the server's ApplicationWithGroupToForceInstallList
            //             -- with *valid* custom variable group

            ApplicationServer appServer = GetAppServerWithInstallationSummariesFromDb();

            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");

            ApplicationServerLogic.SaveForceInstallation(new ServerForceInstallation(appServer, appWithValidGroup));

            SetGlobalFreeze(false);

            ApplicationServerLogic.InstallApplications(appServer);

            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Once());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase4()
        {
            // Use Case #4 -- app group does not exist in the server's ApplicationWithGroupToForceInstallList

            ApplicationServer appServer = GetAppServerWithInstallationSummariesFromDb();

            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");

            SetGlobalFreeze(false);

            // Note: We are *not* adding our app to the *force install* list of the server.
            //       In other words, there is no force installation.

            //bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(appServer, appWithValidGroup);
            //Assert.AreEqual(false, actual);

            ApplicationServerLogic.InstallApplications(appServer);

            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Never());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase5()
        {
            // Use Case #5 -- app exists in the server's ApplicationWithGroupToForceInstallList,
            //                but the custom variable is different

            string rootName = "UseCase05";

            TestHelper.CreateAndPersistEntitiesForAUseCase(rootName, 0);

            ApplicationServer appServer = ApplicationServerLogic.GetByName(TestHelper.GetServerName(rootName));

            ApplicationWithOverrideVariableGroup appWithDifferentGroup = new ApplicationWithOverrideVariableGroup();
            appWithDifferentGroup.Application = appServer.ApplicationsWithOverrideGroup[0].Application;
            // Set the group to something that doesn't already exist within the server...
            appWithDifferentGroup.CustomVariableGroup = TestHelper.CreateCustomVariableGroup(rootName + " " + Guid.NewGuid().ToString());

            ApplicationServerLogic.SaveForceInstallation(new ServerForceInstallation(appServer, appWithDifferentGroup));

            SetGlobalFreeze(false);

            ApplicationServerLogic.InstallApplications(appServer);

            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Never());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase6()
        {
            // Use Case #6 -- app does not exist in the server's ApplicationWithGroupToForceInstallList,
            //                but the custom variable does.

            string rootName = "UseCase06";

            TestHelper.CreateAndPersistEntitiesForAUseCase(rootName, 0);

            ApplicationServer appServer = ApplicationServerLogic.GetByName(TestHelper.GetServerName(rootName));

            appServer.ApplicationsWithOverrideGroup[0].CustomVariableGroup = TestHelper.CreateCustomVariableGroup(rootName);
            ApplicationServerLogic.Save(appServer);  // To save with a valid group.

            // Create a new app group with a new app, but an existing group
            ApplicationWithOverrideVariableGroup appWithValidGroup = new ApplicationWithOverrideVariableGroup();
            appWithValidGroup.Application = TestHelper.CreateApp(rootName + " " + Guid.NewGuid().ToString());
            appWithValidGroup.CustomVariableGroup = appServer.ApplicationsWithOverrideGroup[0].CustomVariableGroup;

            // Set the app to something that doesn't already exist within the server...
            // Leave the group alone because it already exists.

            // Add our app to the force install list of the server
            ServerForceInstallation serverForceInstallation = new ServerForceInstallation(appServer, appWithValidGroup);
            ApplicationServerLogic.SaveForceInstallation(serverForceInstallation);

            SetGlobalFreeze(false);

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(appServer, appWithValidGroup);
            Assert.AreEqual(false, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase7()
        {
            // Use Case #7 -- no force installation exists AT THE APP LEVEL

            // It doesn't really matter what we set here. We're getting rid of the ForceInstallation, so the app shouldn't get installed.
            TestEntityContainer container = CreateTestEntityContainer("UseCase07", x => DateTime.Now, true, false, 0);
            container.AppWithGroup.Application.ForceInstallation = null;

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase8()
        {
            // Use Case #8 -- force installation exists AT THE APP LEVEL, but the force installation time is in the future,
            //                and no installation summaries exist, and the deployment environments match.

            TestEntityContainer container = CreateTestEntityContainer("UseCase08", x => DateTime.Now.AddDays(10), true, false, 0);

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase9()
        {
            // Use Case #9 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                no installation summaries exist, and the deployment environments match.

            TestEntityContainer container = CreateTestEntityContainer("UseCase09", x => DateTime.Now.AddDays(-1), true, false, 0);

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);

            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase10()
        {
            // Use Case #10 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                 no installation summaries exist, and the deployment environments *DO NOT* match.

            TestEntityContainer container = CreateTestEntityContainer("UseCase10", x => DateTime.Now.AddDays(-1), false, false, 0);

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase11()
        {
            // Use Case #11 -- force installation exists AT THE APP LEVEL, and the force installation time is *before* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.

            TestEntityContainer container = CreateTestEntityContainer("UseCase11", x => x.InstallationStart.AddSeconds(-86400),
                true, false, 5);  // 86400 seconds in a day

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(false, actual);  // False because an installation has occurred after the force deployment time.
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase12()
        {
            // Use Case #12 -- force installation exists AT THE APP LEVEL, and the force installation time is *after* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.
            //                 The second test/assert is for when the global setting, FreezeAllInstallations, is true.

            TestEntityContainer container = CreateTestEntityContainer("UseCase12", x => x.InstallationStart.AddSeconds(1), true, false, 5);

            bool actual = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(true, actual);  // True because an installation has not yet occurred after the force deployment time.

            /*************************************************************************************
             * Now try it with FreezeAllInstallations true to override any installation logic.
             *************************************************************************************/

            SetGlobalFreeze(true);

            bool actualUsingFreeze = ApplicationServerLogic.ApplicationShouldBeInstalled(container.ApplicationServer, container.AppWithGroup);
            Assert.AreEqual(false, actualUsingFreeze);  // False because FreezeAllInstallations is true.
        }

        private TestEntityContainer CreateTestEntityContainer(string rootName,
            Func<InstallationSummary, DateTime> forceInstallationTimeFunc,
            bool forceInstallEnvironmentShouldMatch, bool freezeAllInstallations, int numberOfInstallationSummariesToCreate)
        {
            // Creates all of the entities for a particular use case and stores them in a container. This is done
            // because many of the methods in this class do this same thing.

            SetGlobalFreeze(freezeAllInstallations);

            TestHelper.CreateAndPersistEntitiesForAUseCase(rootName, numberOfInstallationSummariesToCreate);

            ApplicationServer appServer = ApplicationServerLogic.GetByName(TestHelper.GetServerName(rootName));

            // So we can check the event log and see that this test passed/failed for the right reason.
            appServer.EnableDebugLogging = _enableAppServerDebugLogging;

            // Use this app and group
            string appName = TestHelper.GetAppName(rootName);
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup.Where(x => x.Application.Name == appName).First();

            // Get the most recent InstallationSummary for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithGroup);

            ForceInstallation forceInstallation     = new ForceInstallation();
            forceInstallation.ForceInstallationTime = forceInstallationTimeFunc.Invoke(mostRecentInstallationSummary);

            if (forceInstallEnvironmentShouldMatch)
            {
                forceInstallation.ForceInstallEnvironment = appServer.InstallationEnvironment;
            }
            else
            {
                forceInstallation.ForceInstallEnvironment =
                    InstallationEnvironmentLogic.GetAll().Where(x => x.LogicalOrder != appServer.InstallationEnvironment.LogicalOrder).First();
            }

            appWithGroup.Application.ForceInstallation = forceInstallation;

            TestEntityContainer container = new TestEntityContainer();

            container.ApplicationServer             = appServer;
            container.AppWithGroup                  = appWithGroup;
            container.ForceInstallation             = forceInstallation;
            container.MostRecentInstallationSummary = mostRecentInstallationSummary;

            return container;
        }

        private static void SetGlobalFreeze(bool shouldFreeze)
        {
            GlobalSetting globalSetting = GlobalSettingLogic.GetItem();

            if (globalSetting == null) { globalSetting = new GlobalSetting(); }

            globalSetting.FreezeAllInstallations = shouldFreeze;

            GlobalSettingLogic.Save(globalSetting);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase13()
        {
            // An install is not supposed to happen. Make sure the installer was not invoked.

            SetGlobalFreeze(false);

            ApplicationServer appServer = GetAppServerWithNoInstallationSummariesFromDb();

            ForceInstallation forceInstallation = new ForceInstallation();
            forceInstallation.ForceInstallationTime = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallEnvironment = // use an environment that's different than our app server's.
                InstallationEnvironmentLogic.GetAll().Where(x => x.LogicalOrder != appServer.InstallationEnvironment.LogicalOrder).First();

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            ApplicationServerLogic.InstallApplications(appServer);
            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Never());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase14()
        {
            // An install is supposed to happen. Make sure the installer was invoked.

            SetGlobalFreeze(false);

            ApplicationServer appServer = GetAppServerWithNoInstallationSummariesFromDb();

            ForceInstallation forceInstallation = new ForceInstallation();
            forceInstallation.ForceInstallationTime = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallEnvironment = appServer.InstallationEnvironment;  // Make environments match

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            ApplicationServerLogic.InstallApplications(appServer);
            _mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Once());
        }

        private static Mock<IAppInstaller> RegisterMockAppInstaller()
        {
            var mockAppInstaller = new Mock<IAppInstaller>();

            // So we don't actually install apps when testing. Just don't do anything.
            mockAppInstaller.Setup(m => m.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()));

            PrestoServerUtility.Container.RegisterInstance<IAppInstaller>(mockAppInstaller.Object);

            return mockAppInstaller;
        }

        private ApplicationServer GetAppServerWithInstallationSummariesFromDb()
        {
            var appServer = ApplicationServerLogic.GetByName("server4");
            appServer.EnableDebugLogging = _enableAppServerDebugLogging;
            return appServer;
        }

        private ApplicationServer GetAppServerWithNoInstallationSummariesFromDb()
        {
            var appServer = ApplicationServerLogic.GetByName("server10");
            appServer.EnableDebugLogging = _enableAppServerDebugLogging;
            return appServer;
        }
    }
}
