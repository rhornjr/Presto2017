using System;
using System.Globalization;
using PrestoCommon.Entities;
using PrestoCommon.Misc;
using PrestoServer.Data.Interfaces;
using Raven.Client;

namespace PrestoServer.Data.RavenDb
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

            using (IDocumentSession session = GetOptimisticSession())
            {
                session.Store(objectToSave, objectToSave.Etag);
                session.SaveChanges();
                SetEtag(objectToSave, session);  // We have a new eTag after saving.
            }
        }

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        public void Delete(EntityBase objectToDelete)
        {
            if (objectToDelete == null) { throw new ArgumentNullException("objectToDelete"); }

            // Can't delete an object that doesn't have an ID. If this ever happens, it's probably because
            // a user created an object, never saved it, and is trying to delete it.
            if (objectToDelete.Id == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture,
                    "Attempted to delete an object that doesn't have an ID. Ignoring delete. Note: This usually happens when " +
                    "a user creates an object, never saved it, and is trying to delete it. Object as string: {0}",
                    objectToDelete.ToString());
                LogUtility.LogWarning(message);
                return;
            }

            using (IDocumentSession session = Database.OpenSession())
            {
                session.Advanced.DocumentStore.DatabaseCommands.Delete(objectToDelete.Id, objectToDelete.Etag);
                session.SaveChanges();
            }
        }
    }
}
