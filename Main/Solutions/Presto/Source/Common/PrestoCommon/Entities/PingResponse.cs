using System;
using Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class PingResponse : EntityBase
    {
        /// <summary>
        /// Gets the ping request id.
        /// </summary>
        public string PingRequestId { get; private set; }

        /// <summary>
        /// Gets the application server id.
        /// </summary>
        public string ApplicationServerId { get; set; }

        /// <summary>
        /// Gets the response time.
        /// </summary>
        public DateTime ResponseTime { get; private set; }

        /// <summary>
        /// Gets the application server.
        /// </summary>
        [JsonIgnore]
        public ApplicationServer ApplicationServer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingResponse"/> class.
        /// </summary>
        public PingResponse() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingResponse"/> class.
        /// </summary>
        /// <param name="pingRequestId">The ping request id.</param>
        /// <param name="responseTime">The response time.</param>
        /// <param name="applicationServer">The application server.</param>
        public PingResponse(string pingRequestId, DateTime responseTime, ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            this.PingRequestId       = pingRequestId;
            this.ApplicationServer   = applicationServer;
            this.ApplicationServerId = applicationServer.Id;
            this.ResponseTime        = responseTime;
        }
    }
}
