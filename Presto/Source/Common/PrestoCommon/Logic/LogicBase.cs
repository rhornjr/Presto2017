using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        public static void Delete(EntityBase objectToDelete)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Delete(objectToDelete);
        }
    }
}
