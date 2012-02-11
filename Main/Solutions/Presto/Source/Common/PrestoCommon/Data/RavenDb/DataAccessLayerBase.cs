﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        protected void PossiblyCloseSession()
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
            if (func == null) { throw new ArgumentNullException("func"); }

            try
            {
                return func.Invoke();
            }
            finally
            {
                Debug.WriteLine("Number of requests just before closing session: " + _session.Advanced.NumberOfRequests);
                PossiblyCloseSession();
            }
        }

        /// <summary>
        /// Queries the single result and cache etags.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        protected static EntityBase QuerySingleResultAndSetEtag(Func<IDocumentSession, EntityBase> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            EntityBase entity = func.Invoke(_session);
            if (entity == null) { return null; }
            SetEtag(entity, _session);
            return entity;
        }

        /// <summary>
        /// Gets all and cache eTags.
        /// </summary>
        /// <param name="func">The func.</param>
        /// <returns></returns>
        // ToDo: Look into this suppression.
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected static IQueryable<EntityBase> QueryAndSetEtags(Func<IDocumentSession, IQueryable<EntityBase>> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            IQueryable<EntityBase> entities = func.Invoke(_session);
            SetEtags(entities, _session);
            return entities;
        }

        /// <summary>
        /// Caches the eTags.
        /// </summary>
        /// <param name="entityBases">The entity bases.</param>
        /// <param name="session">The session.</param>
        protected static void SetEtags(IEnumerable<EntityBase> entityBases, IDocumentSession session)
        {
            if (entityBases == null) { throw new ArgumentNullException("entityBases"); }
            if (session == null) { throw new ArgumentNullException("session"); }

            foreach (EntityBase entityBase in entityBases)
            {
                SetEtag(entityBase, session);
            }
        }

        /// <summary>
        /// Caches the etag.
        /// </summary>
        /// <param name="entityBase">The entity base.</param>
        /// <param name="session">The session.</param>
        protected static void SetEtag(EntityBase entityBase, IDocumentSession session)
        {
            if (entityBase == null) { throw new ArgumentNullException("entityBase"); }
            if (session == null) { throw new ArgumentNullException("session"); }

            entityBase.Etag = (Guid)session.Advanced.GetEtagFor(entityBase);
        }

        private static DocumentStore GetDatabase()
        {            
            DocumentStore documentStore = new DocumentStore();            

            try
            {
                documentStore.ConnectionStringName = "RavenDb";                
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
