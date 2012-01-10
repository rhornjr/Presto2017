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

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <returns></returns>
        public ApplicationServer GetById(string serverId)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="serverIds">The server ids.</param>
        /// <returns></returns>
        public IEnumerable<ApplicationServer> GetByIds(IEnumerable<string> serverIds)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        public void Save(ApplicationServer applicationServer)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Save(applicationServer);
        }        
    }
}
