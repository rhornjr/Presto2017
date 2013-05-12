using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class PingRequestData : DataAccessLayerBase, IPingRequestData
    {
        /// <summary>
        /// Saves the specified ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        public void Save(PingRequest pingRequest)
        {
            new GenericData().Save(pingRequest);
        }

        /// <summary>
        /// Gets the most recent.
        /// </summary>
        /// <returns></returns>
        public PingRequest GetMostRecent()
        {
            return ExecuteQuery<PingRequest>(() =>
                QuerySingleResultAndSetEtag(session => session.Query<PingRequest>()
                    .OrderByDescending(x => x.RequestTime)
                    .Take(1).FirstOrDefault()) as PingRequest);
        }
    }
}
