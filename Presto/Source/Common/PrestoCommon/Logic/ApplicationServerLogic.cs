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
    }
}
