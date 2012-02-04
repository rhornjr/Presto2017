using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
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
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
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
        public void ApplicationShouldBeInstalledTest()
        {
            ApplicationServer_Accessor appServerAccessor = new ApplicationServer_Accessor();

            // If disabled, don't install.
            ApplicationWithOverrideVariableGroup appWithGroup = new ApplicationWithOverrideVariableGroup() { Enabled = false };
            bool actual = appServerAccessor.ApplicationShouldBeInstalled(appWithGroup);
            Assert.AreEqual(false, actual);

            // If enabled, and no installation summaries are found, then we should install.
            appServerAccessor.Id = string.Empty;
            appWithGroup.Enabled = true;
            appWithGroup.Application = new Application() { Id = string.Empty };
            actual = appServerAccessor.ApplicationShouldBeInstalled(appWithGroup);
            Assert.AreEqual(true, actual);

            // We'll need an appServer going forward...
            ApplicationServer appServer = ApplicationServerLogic.GetByName("server4");
            // And we want to give our proxy the same ID and app group
            appServerAccessor.Id = appServer.Id;
            appServerAccessor.ApplicationsWithOverrideGroup.Add(appServer.ApplicationsWithOverrideGroup[0]);
            // Let's make sure we return true for a force install.
            appServerAccessor.ApplicationWithGroupToForceInstallList = new List<ApplicationWithOverrideVariableGroup>();
            appServerAccessor.ApplicationWithGroupToForceInstallList.Add(appServer.ApplicationsWithOverrideGroup[0]);
            // Let's add an installation summary so we can check the rest of the logic in the method.
            InstallationSummary summary = new InstallationSummary(appServer.ApplicationsWithOverrideGroup[0], appServer, DateTime.Now);
            actual = appServerAccessor.ApplicationShouldBeInstalled(appServer.ApplicationsWithOverrideGroup[0]);
            Assert.AreEqual(true, actual);
        }
    }
}
