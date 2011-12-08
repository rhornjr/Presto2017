using System;
using System.Configuration;
using System.Globalization;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        protected static IObjectContainer Database { get; private set; }

        static LogicBase()
        {
            if (Database != null) { return; }

            Database = GetDatabase();
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

            // ToDo: Instead of explicitly setting the update depth above, possibly do this:
            // Use a bunch of these (this isn't so bad because we can add an attribute to every domain entity, then reflectively do this on each one):
            // clientConfig.Common.ObjectClass(typeof(Application)).CascadeOnUpdate(true);
            // clientConfig.Common.ObjectClass(typeof(TaskBase)).CascadeOnUpdate(true);
            // etc...
        }

        /// <summary>
        /// Saves the specified task base.
        /// </summary>
        /// <param name="taskBase">The task base.</param>
        public static void Save(TaskBase taskBase)
        {
            Database.Store(taskBase);
            Database.Commit();
        }
    }
}
