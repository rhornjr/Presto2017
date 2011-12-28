
namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGenericData
    {
        /// <summary>
        /// Saves the specified object to save.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSave">The object to save.</param>
        void Save<T>(T objectToSave);

        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDelete">The object to delete.</param>
        void Delete<T>(T objectToDelete);
    }
}
