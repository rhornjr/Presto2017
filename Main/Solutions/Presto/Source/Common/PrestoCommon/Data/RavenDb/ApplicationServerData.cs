using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.RavenDb
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
            return QueryAndCacheEtags(session => session.Query<ApplicationServer>()).Cast<ApplicationServer>();
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public ApplicationServer GetByName(string serverName)
        {
            return QuerySingleResultAndCacheEtag(session => session.Query<ApplicationServer>()
                .Where(server => server.Name.ToUpperInvariant() == serverName.ToUpperInvariant()).FirstOrDefault())
                as ApplicationServer;
        }
    }
}
