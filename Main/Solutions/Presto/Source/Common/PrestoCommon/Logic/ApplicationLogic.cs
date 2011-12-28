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
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public static void Save(Application application)
        {
            DataAccessFactory.GetDataInterface<IApplicationData>().Save(application);
        }
    }
}
