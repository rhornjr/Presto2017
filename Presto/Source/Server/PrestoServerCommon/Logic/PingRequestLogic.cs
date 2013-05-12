using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class PingRequestLogic
    {
        /// <summary>
        /// Saves the specified ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        public static void Save(PingRequest pingRequest)
        {
            DataAccessFactory.GetDataInterface<IPingRequestData>().Save(pingRequest);
        }

        /// <summary>
        /// Gets the most recent.
        /// </summary>
        /// <returns></returns>
        public static PingRequest GetMostRecent()
        {
            return DataAccessFactory.GetDataInterface<IPingRequestData>().GetMostRecent();
        }
    }
}
