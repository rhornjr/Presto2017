using System;
using PrestoCommon.Data.Interfaces;
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
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSave">The object to save.</param>
        public void Save<T>(T objectToSave)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                session.Store(objectToSave);
                session.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDelete">The object to delete.</param>
        public void Delete<T>(T objectToDelete)
        {
            throw new NotImplementedException();
        }
    }
}
