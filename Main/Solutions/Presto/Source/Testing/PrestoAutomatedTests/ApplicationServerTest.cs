using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Logic;

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

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();
            
            // If disabled, don't install.
            ApplicationWithOverrideVariableGroup appWithGroup = new ApplicationWithOverrideVariableGroup() { Enabled = false };
            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase2()
        {
            // Use Case #2 -- app group exists in the server's ApplicationWithGroupToForceInstallList -- with *null* custom variable group

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();
            
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

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithNullGroup);
            Assert.AreEqual(true, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase3()
        {
            // Use Case #3 -- app group exists in the server's ApplicationWithGroupToForceInstallList -- with *valid* custom variable group

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

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

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(true, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase4()
        {
            // Use Case #4 -- app group does not exist in the server's ApplicationWithGroupToForceInstallList

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

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
            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase5()
        {
            // Use Case #5 -- app group does exist in the server's ApplicationWithGroupToForceInstallList, but the custom variable ID is different

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

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
            
            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase6()
        {
            // Use Case #6 -- app group does not exist in the server's ApplicationWithGroupToForceInstallList, but the custom variable ID exists

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

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
            
            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);

            ApplicationServerLogic.RemoveForceInstallation(serverForceInstallation);  // Clean-up
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase7()
        {
            // Use Case #7 -- no force installation exists AT THE APP LEVEL

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);            

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase8()
        {
            // Use Case #8 -- force installation exists AT THE APP LEVEL, but the force installation time is in the future,
            //                and no installation summaries exist.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            ForceInstallation forceInstallation = new ForceInstallation();
            forceInstallation.ForceInstallationTime = DateTime.Now.AddDays(10);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.DeploymentEnvironment;  // Make sure the environment matches

            appWithValidGroup.Application.ForceInstallation = forceInstallation;

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase9()
        {
            // Use Case #9 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                no installation summaries exist, and the deployment environments match.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");  // Use server 10 because it doesn't have any installation summaries
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.DeploymentEnvironment;  // Make sure the environment matches

            appWithValidGroup.Application.ForceInstallation = forceInstallation;

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(true, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase10()
        {
            // Use Case #10 -- force installation exists AT THE APP LEVEL, and the force installation time is in the past,
            //                 no installation summaries exist, and the deployment environments *DO NOT* match.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server10");  // Use server 10 because it doesn't have any installation summaries
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithValidGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithValidGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById("CustomVariableGroups/4");
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithValidGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = DateTime.Now.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = DeploymentEnvironment.QA;  // Make sure the environments don't match

            appWithValidGroup.Application.ForceInstallation = forceInstallation;

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithValidGroup);
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase11()
        {
            // Use Case #11 -- force installation exists AT THE APP LEVEL, and the force installation time is *before* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");  // Use server 4 because it has installation summaries
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;

            // Use this app
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithNullGroup.CustomVariableGroup = null;
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithNullGroup);

            // Get the list of InstallationStatus entities for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithNullGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = mostRecentInstallationSummary.InstallationStart.AddDays(-1);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.DeploymentEnvironment;  // Make sure the environment matches

            appWithNullGroup.Application.ForceInstallation = forceInstallation;

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithNullGroup);
            Assert.AreEqual(false, actual);  // False because an installation has occurred after the force deployment time.
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase12()
        {
            // Use Case #12 -- force installation exists AT THE APP LEVEL, and the force installation time is *after* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");  // Use server 4 because it has installation summaries
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithNullGroup.CustomVariableGroup = null;
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithNullGroup);

            // Get the list of InstallationStatus entities for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithNullGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = mostRecentInstallationSummary.InstallationStart.AddSeconds(1);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.DeploymentEnvironment;  // Make sure the environment matches

            appWithNullGroup.Application.ForceInstallation = forceInstallation;

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithNullGroup);
            Assert.AreEqual(true, actual);  // True because an installation has not yet occurred after the force deployment time.
        }

        [TestMethod()]
        [DeploymentItem("PrestoCommon.dll")]
        public void ApplicationShouldBeInstalledTest_UseCase13()
        {
            // Use Case #13 -- force installation exists AT THE APP LEVEL, and the force installation time is *after* the most recent
            //                 installation summary time, *installation summaries exist*, and the deployment environments *match*.
            //                 However, the global setting, FreezeAllInstallations, is true.

            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // Use this app server
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");  // Use server 4 because it has installation summaries
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            // Use this app
            ApplicationWithOverrideVariableGroup appWithNullGroup = appServer.ApplicationsWithOverrideGroup[0];
            appWithNullGroup.CustomVariableGroup = null;
            // Add our app to the server
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appWithNullGroup);

            // Get the list of InstallationStatus entities for this server.
            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(appServer, appWithNullGroup);

            ForceInstallation forceInstallation            = new ForceInstallation();
            forceInstallation.ForceInstallationTime        = mostRecentInstallationSummary.InstallationStart.AddSeconds(1);
            forceInstallation.ForceInstallationEnvironment = appServerAccessor.DeploymentEnvironment;  // Make sure the environment matches

            appWithNullGroup.Application.ForceInstallation = forceInstallation;

            // Set FreezeAllInstallations to true to override any installation logic.
            GlobalSetting globalSetting = GlobalSettingLogic.GetItem();
            if (globalSetting == null) { globalSetting = new GlobalSetting(); }
            globalSetting.FreezeAllInstallations = true;
            GlobalSettingLogic.Save(globalSetting);

            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithNullGroup);
            Assert.AreEqual(true, actual);  // True because an installation has not yet occurred after the force deployment time.

            bool actualUsingFreeze = appServerAccessor.FinalInstallationChecksPass(appWithNullGroup);
            Assert.AreEqual(false, actualUsingFreeze);  // False because FreezeAllInstallations is true.
        }
    }
}
