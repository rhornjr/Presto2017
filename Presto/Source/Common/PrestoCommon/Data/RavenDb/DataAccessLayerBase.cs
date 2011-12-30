using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static Dictionary<string, Guid> _entityIdAndEtagMapping;

        /// <summary>
        /// Gets the database.
        /// </summary>
        protected static DocumentStore Database { get; private set; }

        // ToDo: Take care of this.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DataAccessLayerBase()
        {
            if (Database != null) { return; }

            _entityIdAndEtagMapping = new Dictionary<string, Guid>();
            Database = GetDatabase();
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
        /// Queries the single result and cache etags.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        protected static EntityBase QuerySingleResultAndCacheEtag(Func<IDocumentSession, EntityBase> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            using (IDocumentSession session = Database.OpenSession())
            {
                EntityBase entity = func.Invoke(session);
                if (entity == null) { return null; }
                CacheEtag(entity, session);
                return entity;
            }
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

            using (IDocumentSession session = Database.OpenSession())
            {
                IEnumerable<EntityBase> entities = func.Invoke(session);
                CacheEtags(entities, session);
                return entities;
            }
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

            if (!_entityIdAndEtagMapping.ContainsKey(entityBase.Id)) { return Guid.Empty; }

            return _entityIdAndEtagMapping[entityBase.Id];
        }

        private static DocumentStore GetDatabase()
        {
            string databaseUrl = ConfigurationManager.AppSettings["databaseUrl"];            

            DocumentStore documentStore = new DocumentStore();

            try
            {
                //documentStore.ConnectionStringName = "RavenDb";  // See app.config for why this is commented.                
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
