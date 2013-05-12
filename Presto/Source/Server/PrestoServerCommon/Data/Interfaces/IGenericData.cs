using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IGenericData
    {
        /// <summary>
        /// Saves the specified object to save.
        /// </summary>
        /// <param name="objectToSave">The object to save.</param>
        void Save(EntityBase objectToSave);

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        void Delete(EntityBase objectToDelete);
    }
}
