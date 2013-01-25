using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoCommon.Logic;

namespace PrestoAutomatedTests
{
    /// <summary>
    ///This is a test class for LogMessageLogicTest and is intended
    ///to contain all LogMessageLogicTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LogMessageLogicTest
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
        ///A test for GetMostRecentByCreatedTime
        ///</summary>
        [TestMethod()]
        public void GetMostRecentByCreatedTimeTest()
        {
            int numberToRetrieve = 50; // TODO: Initialize to an appropriate value

            // Note: Other tests can create log messages, so we're only going to get the messages that
            //       start with a certain prefix that are part of the standard messages originally loaded.
            IEnumerable<LogMessage> logMessages = new List<LogMessage>(LogMessageLogic.GetMostRecentByCreatedTime(numberToRetrieve))
                .Where(x => x.Message.StartsWith(TestUtility.LogMessagePrefix));

            // Note: The TestUtility will create, say 1000 messages. Each message is "Message n". So the last message
            //       will be "Message 1000".

            int logMessageNumber = TestUtility.TotalNumberOfLogMessages;
            foreach (LogMessage logMessage in logMessages)
            {
                Assert.AreEqual("Message " + logMessageNumber, logMessage.Message);
                logMessageNumber--;  // 1000, 999, 998, etc...
            }
        }
    }
}
