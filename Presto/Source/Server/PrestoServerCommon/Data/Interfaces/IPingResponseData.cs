using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPingResponseData
    {
        /// <summary>
        /// Saves the specified ping response.
        /// </summary>
        /// <param name="pingResponse">The ping response.</param>
        void Save(PingResponse pingResponse);

        /// <summary>
        /// Gets the by ping request and server.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <param name="appServer">The app server.</param>
        /// <returns></returns>
        PingResponse GetByPingRequestAndServer(PingRequest pingRequest, ApplicationServer appServer);

        /// <summary>
        /// Gets all for ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <returns></returns>
        IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest);
    }
}
