using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LogicBase
    {        
        /// <summary>
        /// Saves the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSave">The object to save.</param>
        public static void Save<T>(T objectToSave)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Save(objectToSave);
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDelete">The object to delete.</param>
        public static void Delete<T>(T objectToDelete)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Delete(objectToDelete);
        }
    }
}
