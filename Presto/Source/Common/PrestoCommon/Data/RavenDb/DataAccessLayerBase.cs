using System.Configuration;
using Raven.Client;
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

        /// <summary>
        /// Gets the session.
        /// </summary>
        protected static IDocumentSession Session { get; private set; }

        static DataAccessLayerBase()
        {
            if (Database != null) { return; }

            Database = GetDatabase();
            Session = GetSession();
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

        private static IDocumentSession GetSession()
        {
            IDocumentSession session = Database.OpenSession();

            session.Advanced.UseOptimisticConcurrency = true;

            return session;
        }
    }
}
