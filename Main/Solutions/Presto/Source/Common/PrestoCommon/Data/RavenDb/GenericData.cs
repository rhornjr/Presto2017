using System;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class GenericData : DataAccessLayerBase, IGenericData
    {
        /// <summary>
        /// Saves the specified object to save.
        /// </summary>
        /// <param name="objectToSave">The object to save.</param>
        public void Save(EntityBase objectToSave)
        {
            if (objectToSave == null) { throw new ArgumentNullException("objectToSave"); }

            Guid eTag = Guid.Empty;

            if (objectToSave.Id != null)
            {
                eTag = RetrieveEtagFromCache(objectToSave);
            }

            using (IDocumentSession session = GetOptimisticSession())
            {
                session.Store(objectToSave, eTag);
                session.SaveChanges();
                CacheEtag(objectToSave, session);  // We have a new eTag after saving.
            }
        }

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        public void Delete(EntityBase objectToDelete)
        {
            if (objectToDelete == null) { throw new ArgumentNullException("objectToDelete"); }

            using (IDocumentSession session = Database.OpenSession())
            {
                session.Advanced.DatabaseCommands.Delete(objectToDelete.Id, RetrieveEtagFromCache(objectToDelete));
                session.SaveChanges();
            }
        }
    }
}
