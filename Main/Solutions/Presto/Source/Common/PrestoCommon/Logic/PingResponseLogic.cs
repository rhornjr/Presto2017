using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class PingResponseLogic
    {
        /// <summary>
        /// Saves the specified ping response.
        /// </summary>
        /// <param name="pingResponse">The ping response.</param>
        public static void Save(PingResponse pingResponse)
        {
            DataAccessFactory.GetDataInterface<IPingResponseData>().Save(pingResponse);
        }

        /// <summary>
        /// Gets the by ping request and server.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        public static PingResponse GetByPingRequestAndServer(PingRequest pingRequest, ApplicationServer appServer)
        {
            return DataAccessFactory.GetDataInterface<IPingResponseData>().GetByPingRequestAndServer(pingRequest, appServer);
        }

        /// <summary>
        /// Gets all for ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <returns></returns>
        public static IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            return DataAccessFactory.GetDataInterface<IPingResponseData>().GetAllForPingRequest(pingRequest);
        }
    }
}
