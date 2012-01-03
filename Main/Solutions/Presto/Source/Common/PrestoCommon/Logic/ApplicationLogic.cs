using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationLogic
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Application> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetAll();
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static Application GetByName(string name)
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetByName(name);
        }
    }
}
