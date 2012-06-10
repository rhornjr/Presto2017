using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationServerLogic
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApplicationServer> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetAll();
        }

        /// <summary>
        /// Gets an application server by name.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public static ApplicationServer GetByName(string serverName)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetByName(serverName);
        }

        /// <summary>
        /// Gets the object by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ApplicationServer GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<IApplicationServerData>().GetById(id);
        }

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        public static void Save(ApplicationServer applicationServer)
        {
            DataAccessFactory.GetDataInterface<IApplicationServerData>().Save(applicationServer);
        }
    }
}
