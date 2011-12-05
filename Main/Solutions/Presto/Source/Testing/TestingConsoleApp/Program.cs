using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;
using PrestoCommon.Logic;

namespace TestingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("(R)ead, (W)rite, (B)oth, (C)reate objects, E(x)it");
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.R:
                        TestReadFromDatabase();
                        PressAnyKeyToExit();
                        break;
                    case ConsoleKey.W:
                        TestWriteToDatabase();
                        TestReadFromDatabase();                        
                        PressAnyKeyToExit();
                        break;
                    case ConsoleKey.B:
                        TestWriteToDatabase();
                        TestReadFromDatabase();                        
                        PressAnyKeyToExit();
                        break;
                    case ConsoleKey.C:
                        CreateDatabaseObjects();
                        TestReadFromDatabase();
                        PressAnyKeyToExit();
                        break;
                    default:
                        break;
                }                                               
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }            
        }

        private static void PressAnyKeyToExit()
        {
            Console.WriteLine();
            Console.WriteLine("--done--");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void TestWriteToDatabase()
        {
            Console.WriteLine("Writing to DB...");

            IObjectContainer db = GetDatabase();            

            //UpdateTaskWithinApplication(db);
            //AddNewTaskToApplication(db);
            //AddNewTaskToApplicationLikePrestoDoes(db);
            AddNewTaskToApplicationUsingPrestoLogicClasses();

            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

            db.Close();
        }

        private static void CreateDatabaseObjects()
        {
            IObjectContainer db = GetDatabase();            

            CreateApplicationWithTasks(db);
            CreateServers(db);
            AssociateAppWithServer(db);
            CreateCustomVariables(db);
            AssociateServerWithVariableGroup(db);

            db.Close();
        }

        private static void AddNewTaskToApplication(IObjectContainer db)
        {
            Application application = (from Application app in db
                                       where app.Name == "Derating"
                                       select app).FirstOrDefault();

            TaskDosCommand taskDosCommand = new TaskDosCommand("Task " + DateTime.Now.ToString(), 1, 20, false, "cmd", "/c exit");

            application.Tasks.Add(taskDosCommand);

            db.Store(application);
        }
        
        private static void AddNewTaskToApplicationLikePrestoDoes(IObjectContainer db)
        {
            IEnumerable<Application> applications = from Application app in db
                                                    select app;

            Collection<Application> appCollection = new Collection<Application>(applications.ToList());            

            Application application = appCollection.Where(app => app.Name == "Derating").FirstOrDefault();

            TaskDosCommand taskDosCommand = new TaskDosCommand("Task " + DateTime.Now.ToString(), 1, 20, false, "cmd", "/c exit");

            application.Tasks.Add(taskDosCommand);

            db.Store(application);
        }

        private static void AddNewTaskToApplicationUsingPrestoLogicClasses()
        {
            IObjectContainer db = LogicBase.Database;
            //IObjectContainer db = GetDatabase();

            //Collection<Application> appCollection = new Collection<Application>(ApplicationLogic.GetAll().ToList());
            IEnumerable<Application> applications = from Application app in db
                                                    select app;
            Collection<Application> appCollection = new Collection<Application>(applications.ToList());

            Application selectedApplication = appCollection.Where(app => app.Name == "Derating").FirstOrDefault();

            TaskDosCommand taskDosCommand = new TaskDosCommand("Task " + DateTime.Now.ToString(), 1, 20, false, "cmd", "/c exit");

            selectedApplication.Tasks.Add(taskDosCommand);

            //ApplicationLogic.Save(selectedApplication);
            db.Store(selectedApplication);
            db.Commit();
        }

        private static void UpdateTaskWithinApplication(IObjectContainer db)
        {
            Application application = (from Application app in db
                                       where app.Name == "Derating"
                                       select app).FirstOrDefault();

            foreach (TaskBase task in application.Tasks)
            {
                task.Description += ".";
                db.Store(task);
            }

            // What I found:
            // For an update to work on sub-properties of an object, we need to do one of two things:
            // (1) Call db.store as each sub-property is changed, like when altering the specific task above [Recommended]
            // (2) Explicitly set update depth: db.Ext().Configure().UpdateDepth(5);
            // I thought transparent persistence was supposed to make these steps unnecessary, but it's not working.
            // Is transparent activation necessary? I don't think so, but it may be worth investigating. That' didn't work either.
            // Actually, there is a danger with doing #1: If we need to rollback, we don't want partially-committed objects.
            // No worries here; if we need to rollback, then just call db.Rollback before doing a db.Commit() or db.Close().
            // From http://stackoverflow.com/questions/6803215/db4o-tranparent-persistence-doesnt-store-later-objects-in-my-own-activatablecol:
            // Just use an ActivatableList<T> instead of a List<T>. then everything works fine.

            //db.Ext().Configure().UpdateDepth(5);
            //db.Store(application.Tasks);  // didn't work
            //db.Store(application);
            //db.Rollback();  // This should work because we haven't called Commit() or Close() on db.
            db.Commit();
        }

        private static void AssociateServerWithVariableGroup(IObjectContainer db)
        {
            ApplicationServer server = (from ApplicationServer appServer in db
                                        where appServer.Name == "PbgAppMesD10"
                                        select appServer).FirstOrDefault();

            if (server == null) { return; }

            CustomVariableGroup qaGroup = (from CustomVariableGroup customGroup in db
                                           where customGroup.Name == "QA"
                                           select customGroup).FirstOrDefault();

            if (qaGroup != null) { server.CustomVariableGroups.Add(qaGroup); }

            CustomVariableGroup pbgGroup = (from CustomVariableGroup customGroup in db
                                            where customGroup.Name == "PBG"
                                            select customGroup).FirstOrDefault();

            if (pbgGroup != null) { server.CustomVariableGroups.Add(pbgGroup); }

            if (qaGroup != null || pbgGroup != null)
            {
                db.Store(server);
            }
        }

        private static void CreateCustomVariables(IObjectContainer db)
        {
            CustomVariableGroup customVariableGroup = new CustomVariableGroup() { Name = "QA" };

            customVariableGroup.CustomVariables.Add(new CustomVariable() { Key = "connectionStringSomeDb", Value = "Data Source = QaDbServer; Initial Catalog = SomeDb; Integrated Security = True" });

            customVariableGroup.CustomVariables.Add(new CustomVariable() { Key = "connectionStringAnotherDb", Value = "Data Source = QaDbServer; Initial Catalog = AnotherDb; Integrated Security = True" });

            customVariableGroup.CustomVariables.Add(new CustomVariable() { Key = "serviceAccountUser", Value = @"domain\QaUser" });

            customVariableGroup.CustomVariables.Add(new CustomVariable() { Key = "serviceAccountPassword", Value = @"pw" });

            db.Store(customVariableGroup);

            //CustomVariableGroup pbgGroup = new CustomVariableGroup() { Name = "PBG" };
            //pbgGroup.CustomVariables.Add(new CustomVariable() { Key = "site", Value = "PBG" });

            //CustomVariableGroup ffoGroup = new CustomVariableGroup() { Name = "FFO" };
            //ffoGroup.CustomVariables.Add(new CustomVariable() { Key = "site", Value = "FFO" });

            //CustomVariableGroup klmGroup = new CustomVariableGroup() { Name = "KLM" };
            //klmGroup.CustomVariables.Add(new CustomVariable() { Key = "site", Value = "KLM" });

            //db.Store(pbgGroup);
            //db.Store(ffoGroup);
            //db.Store(klmGroup);
        }

        private static void AssociateAppWithServer(IObjectContainer db)
        {
            string appName = "Derating";

            Application application = (from Application app in db
                                       where app.Name == appName
                                       select app).FirstOrDefault();

            if (application == null)
            {
                Console.WriteLine("AssociateAppWithServer(), app {0} not found.", appName);
                return;
            }

            string appServerName = "DellXps";

            ApplicationServer server = (from ApplicationServer appServer in db
                                        where appServer.Name == appServerName
                                        select appServer).FirstOrDefault();

            if (server == null)
            {
                Console.WriteLine("AssociateAppWithServer(), server {0} not found.", appServerName);
                return;
            }

            //server.IpAddress += "x";
            server.Applications.Add(application);
            //server.Applications.Add(new Application() { Name = "Presto", ReleaseFolderLocation = "somewhere", Version = "2.0" });

            //db.Commit();

            //foreach (Application app in server.Applications)
            //{
            //    db.Store(app);
            //}

            db.Store(server);
        }

        private static void CreateApplicationWithTasks(IObjectContainer db)
        {
            Application application = new Application();

            application.Name                  = "Derating";
            application.ReleaseFolderLocation = @"\\sys02\h\etc...\";
            application.Version               = "1.0.0.0";

            // The DOS command below writes NULL to the file. This is somewhat like the touch command in Linux.
            TaskBase task1 = new TaskDosCommand("Copy files", 1, 1, false, "cmd", @"/c copy /y NUL c:\temp\t1.txt");
            TaskBase task2 = new TaskDosCommand("Alter config", 1, 2, false, "cmd", @"/c copy /y NUL c:\temp\t2.txt");

            application.Tasks.Add(task1);
            application.Tasks.Add(task2);

            db.Store(application);
        }

        private static void CreateServers(IObjectContainer db)
        {
            for (int i = 10; i <= 24; i++)
            {
                ApplicationServer server = new ApplicationServer() { Name = "PbgAppMesD" + i, IpAddress = "10.1.2." + i };
                db.Store(server);
            }

            // For testing on my laptop
            ApplicationServer laptop = new ApplicationServer() { Name = "DellXps", IpAddress = "10.0.0.3" };
            db.Store(laptop);
        }

        private static void TestReadFromDatabase()
        {
            Console.WriteLine("Reading from DB...");

            IObjectContainer db = GetDatabase();
            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

            ReadApplications(db);
            ReadTasks(db);
            ReadServers(db);
            ReadCustomVariables(db);

            db.Close();
        }

        private static void ReadCustomVariables(IObjectContainer db)
        {
            IEnumerable<CustomVariableGroup> groups = from CustomVariableGroup customGroup in db
                                                      select customGroup;

            Console.WriteLine();
            Console.WriteLine("-- All Custom Variable Groups --");
            Console.WriteLine();

            foreach (CustomVariableGroup group in groups)
            {
                Console.WriteLine(group.Name);

                foreach (CustomVariable variable in group.CustomVariables)
                {
                    Console.WriteLine("-- variable: " + variable.Key + ": " + variable.Value);
                }
            }
        }

        private static void ReadApplications(IObjectContainer db)
        {
            IEnumerable<Application> applications = from Application application in db
                                                    //where application.Name == "Derating"
                                                    select application;

            Console.WriteLine();
            Console.WriteLine("-- All Applications --");
            Console.WriteLine();

            int i = 0;
            foreach (Application application in applications)
            {
                i++;
                Console.WriteLine(i.ToString() + ": " + application.Name);

                foreach (TaskBase task in application.Tasks)
                {
                    Console.WriteLine("-- task: " + task.Description);
                }
            }
        }

        private static void ReadTasks(IObjectContainer db)
        {
            IEnumerable<TaskBase> tasks = from TaskBase task in db
                                          select task;

            Console.WriteLine();
            Console.WriteLine("-- All Tasks --");
            Console.WriteLine();

            int i = 0;
            foreach (TaskBase task in tasks)
            {
                i++;
                Console.WriteLine(i.ToString() + ": " + task.Description);
            }
        }

        private static void ReadServers(IObjectContainer db)
        {
            // This didn't work...
            //Server testServer = db.Query<Server>(server => server.Name == "PbgAppMesD04").FirstOrDefault();

            // But this did...
            //Server testServer = (from Server server in db
            //                     where server.Name == "PbgAppMesD04"
            //                     select server).FirstOrDefault();

            //string serverName = "PbgAppMesD10";

            IEnumerable<ApplicationServer> allServers = from ApplicationServer server in db
                                                        //where server.Name == serverName
                                                        select server;

            Console.WriteLine();
            Console.WriteLine("-- All Servers --");
            Console.WriteLine();

            //Server anyServer = db.Query<Server>().FirstOrDefault();            

            int i = 0;
            foreach (ApplicationServer server in allServers)
            {
                i++;
                LogServerInfo(server, i.ToString());

                foreach (Application app in server.Applications)
                {
                    Console.WriteLine("-- {0}", app.Name);
                }
            }

            //LogServerInfo(anyServer, "Any server");
        }

        private static void LogServerInfo(ApplicationServer server, string description)
        {
            if (server == null)
            {
                Console.WriteLine(description + " is null.");
                return;
            }

            Console.WriteLine(string.Format("{0}: {1} - {2}", description, server.Name, server.IpAddress));
        }

        private static IObjectContainer GetDatabase()
        {
            string databaseServerName = ConfigurationManager.AppSettings["databaseServerName"];
            string databaseUser       = ConfigurationManager.AppSettings["databaseUser"];
            string databasePassword   = ConfigurationManager.AppSettings["databasePassword"];
            int databaseServerPort    = Convert.ToInt32(ConfigurationManager.AppSettings["databaseServerPort"], CultureInfo.InvariantCulture);

            IClientConfiguration clientConfig = Db4oClientServer.NewClientConfiguration();            
            clientConfig.Common.UpdateDepth = 10;

            return Db4oClientServer.OpenClient(clientConfig, databaseServerName, databaseServerPort, databaseUser, databasePassword);
        }   
    }
}
