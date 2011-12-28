using PrestoCommon.Data.Interfaces;

namespace PrestoCommon.Data.db4o
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
            Database.Store(objectToSave);
            Database.Commit();
        }

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDelete">The object to delete.</param>
        public void Delete<T>(T objectToDelete)
        {
            Database.Delete(objectToDelete);
            Database.Commit();
        }
    }
}
