using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PrestoCommon.Entities;
using Raven.Client;
using Raven.Client.Document;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DataAccessLayerBase
    {
        private static Dictionary<string, Guid> _entityIdAndEtagMapping = new Dictionary<string, Guid>();
        private static DocumentStore _database = GetDatabase();

        [ThreadStatic]
        private static IDocumentSession _session;

        private bool _isInitialDalInstance;  // This is the DAL method called by the logic class (not other DAL methods).

        /// <summary>
        /// Gets the database.
        /// </summary>
        protected static DocumentStore Database
        {
            get { return _database; }
        }

        /// <summary>
        /// Gets the optimistic session.
        /// </summary>
        /// <returns></returns>
        protected static IDocumentSession GetOptimisticSession()
        {
            IDocumentSession session = Database.OpenSession();

            session.Advanced.UseOptimisticConcurrency = true;

            return session;
        }

        /// <summary>
        /// Sets this instance as the initial DAL instance and creates the session.
        /// </summary>
        public void SetAsInitialDalInstanceAndCreateSession()
        {
            _isInitialDalInstance = true;
            _session = Database.OpenSession();            
        }

        /// <summary>
        /// Closes the session.
        /// </summary>
        protected void CloseSession()
        {
            // Only close the session for the instance that opened it originally.
            if (_isInitialDalInstance == true)
            {
                _session.Dispose();
                _session = null;
            }
        }

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="func">The action.</param>
        protected T ExecuteQuery<T>(Func<T> func)
        {
            // ToDo: I believe this method is now unnecessary. If we add the try/finally to QuerySingleResultAndCacheEtag()
            //       and QueryAndCacheEtags(), then the DAL methods can just call those directly, instead of going through
            //       this method.
            if (func == null) { throw new ArgumentNullException("func"); }

            try
            {
                return func.Invoke();
            }
            finally
            {
                Debug.WriteLine("Number of requests just before closing session: " + _session.Advanced.NumberOfRequests);
                CloseSession();
            }
        }

        /// <summary>
        /// Queries the single result and cache etags.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        protected static EntityBase QuerySingleResultAndCacheEtag(Func<IDocumentSession, EntityBase> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            EntityBase entity = func.Invoke(_session);
            if (entity == null) { return null; }
            CacheEtag(entity, _session);
            return entity;
        }

        /// <summary>
        /// Gets all and cache eTags.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        // ToDo: Look into this suppression.
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected static IEnumerable<EntityBase> QueryAndCacheEtags(Func<IDocumentSession, IEnumerable<EntityBase>> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            IEnumerable<EntityBase> entities = func.Invoke(_session);
            CacheEtags(entities, _session);
            return entities;
        }

        /// <summary>
        /// Caches the eTags.
        /// </summary>
        /// <param name="entityBases">The entity bases.</param>
        /// <param name="session">The session.</param>
        protected static void CacheEtags(IEnumerable<EntityBase> entityBases, IDocumentSession session)
        {
            if (entityBases == null) { throw new ArgumentNullException("entityBases"); }
            if (session == null) { throw new ArgumentNullException("session"); }

            foreach (EntityBase entityBase in entityBases)
            {
                CacheEtag(entityBase, session);
            }
        }

        /// <summary>
        /// Caches the etag.
        /// </summary>
        /// <param name="entityBase">The entity base.</param>
        /// <param name="session">The session.</param>
        protected static void CacheEtag(EntityBase entityBase, IDocumentSession session)
        {
            if (entityBase == null) { throw new ArgumentNullException("entityBase"); }
            if (session == null) { throw new ArgumentNullException("session"); }

            Guid eTag = (Guid)session.Advanced.GetEtagFor(entityBase);

            if (_entityIdAndEtagMapping.ContainsKey(entityBase.Id))
            {
                _entityIdAndEtagMapping[entityBase.Id] = eTag;  // Replace/update existing
                return; 
            }
            _entityIdAndEtagMapping.Add(entityBase.Id, eTag);  // Add new
        }

        /// <summary>
        /// Retrieves the etag from cache.
        /// </summary>
        /// <param name="entityBase">The entity base.</param>
        /// <returns></returns>
        protected static Guid RetrieveEtagFromCache(EntityBase entityBase)
        {
            if (entityBase == null) { throw new ArgumentNullException("entityBase"); }

            if (entityBase.Id == null || !_entityIdAndEtagMapping.ContainsKey(entityBase.Id)) { return Guid.Empty; }

            return _entityIdAndEtagMapping[entityBase.Id];
        }

        private static DocumentStore GetDatabase()
        {            
            DocumentStore documentStore = new DocumentStore();            

            try
            {
                documentStore.Conventions.MaxNumberOfRequestsPerSession = int.MaxValue;  // * See notes below.
                documentStore.ConnectionStringName = "RavenDb";                
                documentStore.Initialize();                

                // *By default, Raven sets the max requests to 128. That makes queries, like getting the 50 most recent installation
                // summaries, not work. When the limit is 128, queries are only evaluated against the first 128 documents
                // in a table. Not cool.
                // Note: This still didn't work when I was using a Lucene query:
                //         session.Advanced.LuceneQuery<InstallationSummary>()
                //       I had to do this:
                //         using Raven.Client.Linq;
                //         session.Query<InstallationSummary>()
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
