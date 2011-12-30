using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

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
        /// <param name="objectToSave">The object to save.</param>
        public void Save(EntityBase objectToSave)
        {
            Database.Store(objectToSave);
            Database.Commit();
        }

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        public void Delete(EntityBase objectToDelete)
        {
            Database.Delete(objectToDelete);
            Database.Commit();
        }
    }
}
