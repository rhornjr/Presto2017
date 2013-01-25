using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrestoCommon.Entities;
using PrestoCommon.Factories;
using PrestoCommon.Factories.OpenFileDialog;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel;
using PrestoViewModel.Tabs;
using Microsoft.Practices.Unity;

namespace PrestoAutomatedTests
{
    /// <summary>
    ///This is a test class for ApplicationViewModelTest and is intended
    ///to contain all ApplicationViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ApplicationViewModelTest
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
        ///A test for ImportTasks
        ///</summary>
        //[TestMethod()]
        [DeploymentItem("PrestoViewModel.dll")]
        public void ImportTasksTest()
        {
            // Need this because it assigns itself as the main window in ViewModelUtility.
            MainWindowViewModel mainViewModel = new MainWindowViewModel();

            // Use a mock open file dialog so we don't need user input.
            CommonUtility.Container.RegisterType<IOpenFileDialogService, MockOpenFileDialogService>();
            MockOpenFileDialogService.SetFileName(@"C:\Data\Presto2Files\Presto2ToRunAtHome\Derating.Tasks");

            ApplicationViewModel_Accessor appViewModel = new ApplicationViewModel_Accessor();            

            DateTime now = DateTime.Now;

            Application app = new Application();
            app.Name = "App " + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + "_" +
                now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString();

            ApplicationLogic.Save(app);

            appViewModel.SelectedApplication = app;

            appViewModel.ImportTasks();

            // Need to verify that the app has x number of tasks.
        }
    }
}
