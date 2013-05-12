using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoServer.Logic;

namespace PrestoAutomatedTests
{
    /// <summary>
    ///This is a test class for ApplicationLogicTest and is intended
    ///to contain all ApplicationLogicTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationLogicTest
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

        // Use ClassInitialize to run code before running the first test in the class
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
        
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        
        #endregion

        /// <summary>
        ///A test for GetByName
        ///</summary>
        [TestMethod()]
        public void GetByNameTest()
        {
            string name    = "app4";
            string version = "1.0.0.4";

            Application app = ApplicationLogic.GetByName(name);

            Assert.AreEqual(name, app.Name);
            Assert.AreEqual(version, app.Version);
        }

        // GetAll() methods aren't working now that we have other tests adding new data.
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    List<Application> allApps = new List<Application>(ApplicationLogic.GetAll());

        //    Assert.AreEqual(TestUtility.TotalNumberOfEachEntityToCreate, allApps.Count);
        //}

        [TestMethod()]
        public void GetByIdTest()
        {
            string name = "app4";

            Application appByName = ApplicationLogic.GetByName(name);

            Application appById = ApplicationLogic.GetById(appByName.Id);

            Assert.AreEqual(appByName.Id, appById.Id);
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveNullApplicationTest()
        {
            ApplicationLogic.Save(null);
        }
    }
}
