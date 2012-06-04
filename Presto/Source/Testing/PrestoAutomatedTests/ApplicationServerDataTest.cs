using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Data.SqlServer;
using PrestoCommon.Entities;
using PrestoCommon.Enums;

namespace PrestoAutomatedTests
{
    
    
    /// <summary>
    ///This is a test class for ApplicationServerDataTest and is intended
    ///to contain all ApplicationServerDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationServerDataTest
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


        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SaveNullServer()
        {
            ApplicationServerData data = new ApplicationServerData();
            ApplicationServer appServer = null;

            data.SaveTest(appServer);
        }

        [TestMethod()]
        public void AddNewServerScalarPropertiesOnly()
        {
            ApplicationServerData data = new ApplicationServerData();

            string serverName = "Dev Server 1";

            ApplicationServer appServer = CreateScalarDevServer(serverName);

            data.SaveTest(appServer);

            ApplicationServer serverFromDb = data.GetByName(serverName);

            Assert.IsTrue(ServersMatch(appServer, serverFromDb));
        }

        [TestMethod()]
        public void AddNewServerScalarPlusExistingGroup()
       { 
            string serverName = "Dev Server 2";

            ApplicationServer appServer = CreateScalarDevServer(serverName);

            CustomVariableGroup group = SaveNewCustomVariableGroup("Group 1");

            appServer.CustomVariableGroups.Add(group);

            ApplicationServerData data = new ApplicationServerData();
            ApplicationServer serverFromDb = data.SaveTest(appServer);

            Assert.IsTrue(ServersMatch(appServer, serverFromDb));
        }

        [TestMethod]
        public void UpdateExistingServerName()
        {
            string serverName = "Dev Server 3";
            string groupName = "Group 3";
            ApplicationServer appServer = CreateServerWithExistingGroup(serverName, groupName);

            appServer.Name = serverName + "x";

            ApplicationServerData data = new ApplicationServerData();
            ApplicationServer serverFromDb = data.SaveTest(appServer);

            Assert.IsTrue(ServersMatch(appServer, serverFromDb));
        }

        private ApplicationServer CreateServerWithExistingGroup(string serverName, string groupName)
        {
            ApplicationServer appServer = CreateScalarDevServer(serverName);

            CustomVariableGroup group = SaveNewCustomVariableGroup(groupName);

            appServer.CustomVariableGroups.Add(group);

            ApplicationServerData data = new ApplicationServerData();

            return data.SaveTest(appServer);
        }

        private static CustomVariableGroup SaveNewCustomVariableGroup(string groupName)
        {
            CustomVariableGroup group = new CustomVariableGroup();

            group.Name = groupName;

            group.CustomVariables.Add(new CustomVariable() { Key = "Key 1", Value = "Value 1" });

            CustomVariableGroupData data = new CustomVariableGroupData();

            data.Save(group);

            group.IdForEf = 1; // test
            return group; // test

            //return data.GetByName(groupName);
        }

        private static ApplicationServer CreateScalarDevServer(string serverName)
        {
            ApplicationServer appServer     = new ApplicationServer();
            appServer.DeploymentEnvironment = DeploymentEnvironment.Development;
            appServer.Description           = "Dev Server";
            appServer.EnableDebugLogging    = false;
            appServer.Name                  = serverName;

            return appServer;
        }

        private static bool ServersMatch(ApplicationServer server1, ApplicationServer server2)
        {
            if (server1.Name                  != server2.Name) { return false; }
            if (server1.EnableDebugLogging    != server2.EnableDebugLogging) { return false; }
            if (server1.Description           != server2.Description) { return false; }
            if (server1.DeploymentEnvironment != server2.DeploymentEnvironment) { return false; }

            if (!ServerGroupsMatch(server1, server2)) { return false; }

            return true;
        }

        private static bool ServerGroupsMatch(ApplicationServer server1, ApplicationServer server2)
        {
            if (server1.CustomVariableGroups == null && server2.CustomVariableGroups != null) { return false; }
            if (server1.CustomVariableGroups != null && server2.CustomVariableGroups == null) { return false; }

            if (server1.CustomVariableGroups == null) { return true; }

            if (server1.CustomVariableGroups.Count != server2.CustomVariableGroups.Count) { return false; }

            foreach (CustomVariableGroup groupInServer1 in server1.CustomVariableGroups)
            {
                CustomVariableGroup groupInServer2 = server2.CustomVariableGroups.Single(x => x.IdForEf == groupInServer1.IdForEf);
                if (!GroupsMatch(groupInServer1, groupInServer1)) { return false; }
            }

            return true;
        }

        private static bool GroupsMatch(CustomVariableGroup group1, CustomVariableGroup group2)
        {
            if (group1.Name != group2.Name) { return false; }

            if (!CustomVariablesMatch(group1.CustomVariables, group2.CustomVariables)) { return false; }

            return true;
        }

        private static bool CustomVariablesMatch(ObservableCollection<CustomVariable> variables1, ObservableCollection<CustomVariable> variables2)
        {
            if (variables1 == null && variables2 != null) { return false; }
            if (variables1 != null && variables2 == null) { return false; }

            if (variables1 == null) { return true; }

            foreach (CustomVariable variableIn1 in variables1)
            {
                CustomVariable variableIn2 = variables2.Single(x => x.Key == variableIn1.Key);
                if (variableIn1.Value != variableIn2.Value) { return false; }
            }

            return true;
        }
    }
}
