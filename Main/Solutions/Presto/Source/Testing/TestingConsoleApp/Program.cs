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

            for (int i = 10; i <= 24; i++)
            {
                ApplicationServer server = new ApplicationServer() { Name = "PbgAppMesD" + i, IpAddress = "10.1.2." + i };
                db.Store(server);
            }

            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

            db.Close();
        }

        private static void TestReadFromDatabase()
        {
            Console.WriteLine("Reading from DB...");

            IObjectContainer db = GetDatabase();
            Console.WriteLine(string.Format("db4o server DB closed: {0}", db.Ext().IsClosed().ToString()));

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

            db.Close();
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
            string databaseUser     = ConfigurationManager.AppSettings["databaseUser"];
            string databasePassword = ConfigurationManager.AppSettings["databasePassword"];
            int databaseServerPort  = Convert.ToInt32(ConfigurationManager.AppSettings["databaseServerPort"], CultureInfo.InvariantCulture);

            return Db4oClientServer.OpenClient("localhost", databaseServerPort, databaseUser, databasePassword);
        }   
    }
}
