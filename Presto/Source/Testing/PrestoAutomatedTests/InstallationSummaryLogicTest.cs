using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoCommon.Logic;

namespace PrestoAutomatedTests
{
    
    
    /// <summary>
    ///This is a test class for InstallationSummaryLogicTest and is intended
    ///to contain all InstallationSummaryLogicTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InstallationSummaryLogicTest
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
        ///A test for GetByServerAppAndGroup
        ///</summary>
        [TestMethod()]
        public void GetByServerAppAndGroupTest()
        {            
            string serverName = "server4";
            ApplicationServer server = ApplicationServerLogic.GetByName(serverName);

            string appName = "app8";
            Application app = ApplicationLogic.GetByName(appName);

            ApplicationWithOverrideVariableGroup appWithGroup = new ApplicationWithOverrideVariableGroup();
            appWithGroup.Application = app;

            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(server, appWithGroup);

            InstallationSummary mostRecentSummaryInMemory =
                TestUtility.AllInstallationSummaries
                    .Where(summary => summary.ApplicationServer.Id == server.Id &&
                            summary.ApplicationWithOverrideVariableGroup.ApplicationId == appWithGroup.Application.Id &&
                            summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null)
                    .OrderByDescending(x => x.InstallationStart)
                    .First();

            Assert.AreEqual(mostRecentSummaryInMemory.InstallationStart, mostRecentInstallationSummary.InstallationStart);
            Assert.AreEqual(serverName, mostRecentInstallationSummary.ApplicationServer.Name);
            Assert.AreEqual(appName, mostRecentInstallationSummary.ApplicationWithOverrideVariableGroup.Application.Name);
        }

        /// <summary>
        ///A test for GetMostRecentByStartTime
        ///</summary>
        [TestMethod()]
        public void GetMostRecentByStartTimeTest()
        {
            List<InstallationSummary> summariesFromDb = new List<InstallationSummary>(InstallationSummaryLogic.GetMostRecentByStartTime(50));

            List<InstallationSummary> summariesCreatedByTestUtility =
                new List<InstallationSummary>(TestUtility.AllInstallationSummaries.OrderByDescending(x => x.InstallationStart).Take(50));

            for (int i = 0; i <= 49; i++)
            {
                Assert.AreEqual(summariesCreatedByTestUtility[i].InstallationStart, summariesFromDb[i].InstallationStart);
            }
        }

        [TestMethod()]
        public void GetByServerAppAndGroupWithManyInstallationSummariesTest()
        {
            string serverName = "server4";
            ApplicationServer server = ApplicationServerLogic.GetByName(serverName);

            string appName = "app8";
            Application app = ApplicationLogic.GetByName(appName);

            ApplicationWithOverrideVariableGroup appWithGroup = new ApplicationWithOverrideVariableGroup();
            appWithGroup.Application = app;

            InstallationSummary mostRecentInstallationSummary = InstallationSummaryLogic.GetMostRecentByServerAppAndGroup(server, appWithGroup);

            Assert.AreEqual(serverName, mostRecentInstallationSummary.ApplicationServer.Name);
            Assert.AreEqual(appName, mostRecentInstallationSummary.ApplicationWithOverrideVariableGroup.Application.Name);
        }
    }
}
