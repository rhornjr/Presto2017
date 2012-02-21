using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoCommon.Logic;

namespace PrestoAutomatedTests
{
    
    
    /// <summary>
    ///This is a test class for ApplicationServerLogicTest and is intended
    ///to contain all ApplicationServerLogicTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationServerLogicTest
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
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void GetAllTest()
        {
            List<ApplicationServer> servers = new List<ApplicationServer>(ApplicationServerLogic.GetAll());

            Assert.AreEqual(TestUtility.TotalNumberOfEachEntityToCreate, servers.Count);
        }

        /// <summary>
        ///A test for GetByName
        ///</summary>
        [TestMethod()]
        public void GetByNameTest()
        {
            string name = "server2";

            ApplicationServer server = ApplicationServerLogic.GetByName(name);

            Assert.AreEqual(name, server.Name);
        }
    }
}
