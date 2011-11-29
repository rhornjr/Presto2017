using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace TestingConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //TestWriteToDatabase();
                TestReadFromDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void TestWriteToDatabase()
        {
            Console.WriteLine("Writing to DB...");

            IObjectContainer db = GetDatabase();

            //CreateApplicationWithTasks(db);
            //CreateServers(db);

            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

            db.Close();
        }

        private static void CreateApplicationWithTasks(IObjectContainer db)
        {
            Application application = new Application();

            application.Name                  = "Sample app";
            application.ReleaseFolderLocation = @"c:\temp\";
            application.Version               = "1.0.0.42";

            // The DOS command below writes NULL to the file. This is somewhat like the touch command in Linux.
            TaskBase task1 = new TaskDosCommand("Test task 1a", 1, 1, false, @"cmd", "/c copy /y NUL c:\temp\test1a.txt");
            TaskBase task2 = new TaskDosCommand("Test task 2a", 1, 1, false, @"cmd", "/c copy /y NUL c:\temp\test2a.txt");

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
        }

        private static void TestReadFromDatabase()
        {
            Console.WriteLine("Reading from DB...");

            IObjectContainer db = GetDatabase();
            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

            ReadApplications(db);
            //ReadTasks(db);
            //ReadServers(db);

            db.Close();
        }

        private static void ReadApplications(IObjectContainer db)
        {
            IEnumerable<Application> applications = from Application application in db
                                             select application;

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

            IEnumerable<ApplicationServer> allServers = from ApplicationServer server in db
                                                        select server;

            //Server anyServer = db.Query<Server>().FirstOrDefault();            

            int i = 0;
            foreach (ApplicationServer server in allServers)
            {
                i++;
                LogServerInfo(server, i.ToString());
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

            return Db4oClientServer.OpenClient(databaseServerName, databaseServerPort, databaseUser, databasePassword);
        }   
    }
}
