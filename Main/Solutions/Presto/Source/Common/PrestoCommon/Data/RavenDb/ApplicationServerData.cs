using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

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
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<ApplicationServer>();
            }
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public ApplicationServer GetByName(string serverName)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<ApplicationServer>()
                    .Where(server => server.Name.ToUpperInvariant() == serverName.ToUpperInvariant()).FirstOrDefault();
            }
        }
    }
}
