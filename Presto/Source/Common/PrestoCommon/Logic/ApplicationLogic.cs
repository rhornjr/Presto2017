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

        /// <summary>
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public static void Save(Application application)
        {
            DataAccessFactory.GetDataInterface<IApplicationData>().Save(application);
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Application GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetById(id);
        }
    }
}
