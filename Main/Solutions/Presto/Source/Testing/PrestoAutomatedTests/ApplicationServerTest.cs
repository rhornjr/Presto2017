using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Logic;
using PrestoCommon.Misc;

namespace PrestoAutomatedTests
{
    /// <summary>
    ///This is a test class for ApplicationServerTest and is intended
    ///to contain all ApplicationServerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationServerTest
    {
        private TestContext testContextInstance;
        bool _enableAppServerDebugLogging = true;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
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

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);
            
            // If disabled, don't install.
            ApplicationWithOverrideVariableGroup appWithGroup = new ApplicationWithOverrideVariableGroup() { Enabled = false };
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase2()
        {
            // Use Case #2 -- app group exists in the server's ApplicationWithGroupToForceInstallList -- with *null* custom variable group

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);
            
            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithNullGroup);
            
            // Add our app to the force install list of the server
            ServerForceInstallation serverForceInstallation = new ServerForceInstallation(appServer, appWithNullGroup);
            ApplicationServerLogic.SaveForceInstallation(serverForceInstallation);

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithNullGroup);
            Assert.AreEqual(true, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase3()
        {
            // Use Case #3 -- app group exists in the server's ApplicationWithGroupToForceInstallList -- with *valid* custom variable group

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            // Add our app to the force install list of the server
            ServerForceInstallation serverForceInstallation = new ServerForceInstallation(appServer, appWithValidGroup);
            ApplicationServerLogic.SaveForceInstallation(serverForceInstallation);

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(true, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase4()
        {
            // Use Case #4 -- app group does not exist in the server's ApplicationWithGroupToForceInstallList

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);
            // Note: We are *not* adding our app to the *force install* list of the server
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase5()
        {
            // Use Case #5 -- app group does exist in the server's ApplicationWithGroupToForceInstallList, but the custom variable ID is different

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);
            // Add an app, with the same app ID, but different group ID, to the force install list of the server
            ApplicationWithOverrideVariableGroup appWithDifferentGroup = new ApplicationWithOverrideVariableGroup();
            appWithDifferentGroup.Application = appWithValidGroup.Application;
            appWithDifferentGroup.CustomVariableGroup = null;

            // Add our app to the force install list of the server
            ServerForceInstallation serverForceInstallation = new ServerForceInstallation(appServer, appWithDifferentGroup);
            ApplicationServerLogic.SaveForceInstallation(serverForceInstallation);

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(false, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase6()
        {
            // Use Case #6 -- app group does not exist in the server's ApplicationWithGroupToForceInstallList, but the custom variable ID exists

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            // Add an app, with a different app ID, but the same group ID, to the force install list of the server
            ApplicationWithOverrideVariableGroup appWithDifferentAppId = new ApplicationWithOverrideVariableGroup();
            appWithDifferentAppId.Application = ApplicationLogic.GetById("Applications/3");
            appWithDifferentAppId.CustomVariableGroup = appWithValidGroup.CustomVariableGroup;

            // Add our app to the force install list of the server
            ServerForceInstallation serverForceInstallation = new ServerForceInstallation(appServer, appWithDifferentAppId);
            ApplicationServerLogic.SaveForceInstallation(serverForceInstallation);

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(false, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase7()
        {
            // Use Case #7 -- no force installation exists AT THE APP LEVEL

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase8()
        {
            // Use Case #8 -- force installation exists AT THE APP LEVEL, but the force installation time is in the future,
            //                and no installation summaries exist.

            ApplicationServer appServerAccessor = new ApplicationServer();
            PrivateObject privateObject = new PrivateObject(appServerAccessor);

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = DateTime.Now.AddDays(10);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.InstallationEnvironment;  // Make sure the environment matches

            appWithValidGroup.Application.ForceInstallation = forceInstallation;

            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase9()
        {
            // Use Case #9 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                no installation summaries exist, and the deployment environments match.

            // Prerequisites:
            // An app has a ForceInstallation, and that ForceInstallation environment must match the app server env.
            // The app server must have an appWithGroup for that of our application.

            // Use server 10 because it doesn't have any installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = appServer.InstallationEnvironment;  // Make environments match

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            PrivateObject privateObject = new PrivateObject(appServer);
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithGroup);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase10()
        {
            // Use Case #10 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                 no installation summaries exist, and the deployment environments *DO NOT* match.

            // Prerequisites:
            // An app has a ForceInstallation, and that ForceInstallation environment must match the app server env.
            // The app server must have an appWithGroup for that of our application.

            // Use server 10 because it doesn't have any installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");

            ForceInstallation forceInstallation               = new ForceInstallation();
            forceInstallation.ForceInstallationTime           = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment    = // use an environment that's different than our app server's.
                InstallationEnvironmentLogic.GetAll().Where(x => x.LogicalOrder != appServer.InstallationEnvironment.LogicalOrder).First();

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            PrivateObject privateObject = new PrivateObject(appServer);
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase11()
        {
            // Use Case #11 -- force installation exists AT THE APP LEVEL, and the force installation time is *before* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.

            // Use server 4 because it has installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            appServer.EnableDebugLogging = _enableAppServerDebugLogging;  // So we can check the event log and see that this test passed/failed for the right reason.

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];            

            // Get the most recent InstallationSummary for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = mostRecentInstallationSummary.InstallationStart.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = appServer.InstallationEnvironment;  // Make environments match            

            appWithGroup.Application.ForceInstallation = forceInstallation;

            PrivateObject privateObject = new PrivateObject(appServer);
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithGroup);
            Assert.AreEqual(false, actual);  // False because an installation has occurred after the force deployment time.
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase12()
        {
            // Use Case #12 -- force installation exists AT THE APP LEVEL, and the force installation time is *after* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.
            //                 The second test/assert is for when the global setting, FreezeAllInstallations, is true.

            // Use server 4 because it has installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            appServer.EnableDebugLogging = _enableAppServerDebugLogging;  // So we can check the event log and see that this test passed/failed for the right reason.

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];

            // Get the most recent InstallationSummary for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = mostRecentInstallationSummary.InstallationStart.AddSeconds(1);
            forceInstallation.ForceInstallationEnvironment = appServer.InstallationEnvironment;  // Make environments match            

            appWithGroup.Application.ForceInstallation = forceInstallation;

            PrivateObject privateObject = new PrivateObject(appServer);
            bool actual = (bool)privateObject.Invoke("ApplicationShouldBeInstalled", appWithGroup);
            Assert.AreEqual(true, actual);  // True because an installation has not yet occurred after the force deployment time.

            // Now try it with FreezeAllInstallations true to override any installation logic.
            GlobalSetting globalSetting = GlobalSettingLogic.GetItem();
            if (globalSetting == null) { globalSetting = new GlobalSetting(); }
            globalSetting.FreezeAllInstallations = true;
            GlobalSettingLogic.Save(globalSetting);

            bool actualUsingFreeze = (bool)privateObject.Invoke("FinalInstallationChecksPass", appWithGroup);
            Assert.AreEqual(false, actualUsingFreeze);  // False because FreezeAllInstallations is true.
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase13()
        {
            // An install is not supposed to happen. Make sure the installer was not invoked.

            var mockAppInstaller = RegisterMockAppInstaller();

            // Use server 10 because it doesn't have any installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");

            ForceInstallation forceInstallation = new ForceInstallation();
            forceInstallation.ForceInstallationTime = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = // use an environment that's different than our app server's.
                InstallationEnvironmentLogic.GetAll().Where(x => x.LogicalOrder != appServer.InstallationEnvironment.LogicalOrder).First();

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            appServer.InstallApplications();            
            mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Never());
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase14()
        {
            // An install is supposed to happen. Make sure the installer was invoked.

            var mockAppInstaller = RegisterMockAppInstaller();

            // Use server 10 because it doesn't have any installation summaries
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");

            ForceInstallation forceInstallation = new ForceInstallation();
            forceInstallation.ForceInstallationTime = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = appServer.InstallationEnvironment;  // Make environments match

            // Use this app and group
            ApplicationWithOverrideVariableGroup appWithGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithGroup.Application.ForceInstallation = forceInstallation;

            appServer.InstallApplications();
            mockAppInstaller.Verify(x => x.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()), Times.Once());
        }

        private static Mock<IAppInstaller> RegisterMockAppInstaller()
        {
            var mockAppInstaller = new Mock<IAppInstaller>();

            // So we don't actually install apps when testing. Just don't do anything.
            mockAppInstaller.Setup(m => m.InstallApplication(It.IsAny<ApplicationServer>(),
                It.IsAny<ApplicationWithOverrideVariableGroup>()));

            CommonUtility.Container.RegisterInstance<IAppInstaller>(mockAppInstaller.Object);

            return mockAppInstaller;
        }
    }
}
