using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client.Linq;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class PingResponseData : DataAccessLayerBase, IPingResponseData
    {
        /// <summary>
        /// Saves the specified ping response.
        /// </summary>
        /// <param name="pingResponse">The ping response.</param>
        public void Save(PingResponse pingResponse)
        {
            if (pingResponse == null) { throw new ArgumentNullException("pingResponse"); }

            pingResponse.ApplicationServerId = pingResponse.ApplicationServer.Id;

            new GenericData().Save(pingResponse);
        }

        /// <summary>
        /// Gets the by ping request and server.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <param name="appServer"></param>
        /// <returns></returns>
        public PingResponse GetByPingRequestAndServer(PingRequest pingRequest, ApplicationServer appServer)
        {
            return ExecuteQuery<PingResponse>(() =>
            {
                PingResponse pingResponse = QuerySingleResultAndSetEtag(session => session.Query<PingResponse>()
                    .Include(response => response.ApplicationServerId)
                    .Where(response => response.PingRequestId == pingRequest.Id && response.ApplicationServerId == appServer.Id)
                    .FirstOrDefault()) as PingResponse;

                if (pingResponse != null) { HydratePingResponse(pingResponse); }

                return pingResponse;
            });
        }

        /// <summary>
        /// Gets all for ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        /// <returns></returns>
        public IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            return ExecuteQuery<IEnumerable<PingResponse>>(() =>
            {
                IEnumerable<PingResponse> pingResponses = QueryAndSetEtags(session => session.Query<PingResponse>()
                    .Include(x => x.ApplicationServerId)
                    .Where(x => x.PingRequestId == pingRequest.Id))
                    .AsEnumerable().Cast<PingResponse>();

                foreach (PingResponse pingResponse in pingResponses)
                {
                    HydratePingResponse(pingResponse);
                }

                return pingResponses;
            });
        }

        private static void HydratePingResponse(PingResponse pingResponse)
        {
            pingResponse.ApplicationServer = QuerySingleResultAndSetEtag(
                session => session.Load<ApplicationServer>(pingResponse.ApplicationServerId)) as ApplicationServer;
        }
    }
}
