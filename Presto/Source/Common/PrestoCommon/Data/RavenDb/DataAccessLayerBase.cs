using System.Configuration;
using Raven.Client.Document;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataAccessLayerBase
    {
        /// <summary>
        /// Gets the database.
        /// </summary>
        protected static DocumentStore Database { get; private set; }

        static DataAccessLayerBase()
        {
            if (Database != null) { return; }

            Database = GetDatabase();
        }

        private static DocumentStore GetDatabase()
        {
            string databaseUrl = ConfigurationManager.AppSettings["databaseUrl"];            

            DocumentStore documentStore = new DocumentStore();

            try
            {
                documentStore.Url = databaseUrl;
                documentStore.Initialize();
            }
            catch
            {
                documentStore.Dispose();
                throw;
            }

            return documentStore;
        }
    }
}
