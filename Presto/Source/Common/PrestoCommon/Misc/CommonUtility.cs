using System;
using System.Configuration;
using System.Globalization;
using System.Net.Sockets;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;

namespace PrestoCommon.Misc
{
    /// <summary>
    /// Helper methods for all of Presto to use
    /// </summary>
    public static class CommonUtility
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <returns></returns>        
        /// <exception cref="SocketException"/>
        public static IObjectContainer GetDatabase()
        {
            string databaseServerName = ConfigurationManager.AppSettings["databaseServerName"];
            string databaseUser       = ConfigurationManager.AppSettings["databaseUser"];
            string databasePassword   = ConfigurationManager.AppSettings["databasePassword"];
            int databaseServerPort    = Convert.ToInt32(ConfigurationManager.AppSettings["databaseServerPort"], CultureInfo.InvariantCulture);

            return Db4oClientServer.OpenClient(databaseServerName, databaseServerPort, databaseUser, databasePassword);
        }   
    }
}
