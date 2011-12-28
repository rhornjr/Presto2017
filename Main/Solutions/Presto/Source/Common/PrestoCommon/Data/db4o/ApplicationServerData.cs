using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationServer> GetAll()
        {
            IEnumerable<ApplicationServer> servers = from ApplicationServer server in Database
                                                     select server;

            Database.Ext().Refresh(servers, 10);

            return servers;
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public ApplicationServer GetByName(string serverName)
        {
            ApplicationServer appServer = (from ApplicationServer server in Database
                                           where server.Name.ToUpperInvariant() == serverName.ToUpperInvariant()
                                           select server).FirstOrDefault();

            Database.Ext().Refresh(appServer, 10);

            return appServer;
        }
    }
}
