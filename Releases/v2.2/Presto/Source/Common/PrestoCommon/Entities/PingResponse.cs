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
        /// Gets or sets the comment.
        /// </summary>
        /// <value>
        /// The comment.
        /// </value>
        public string Comment { get; set; }

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
        /// <param name="comment">The comment.</param>
        public PingResponse(string pingRequestId, DateTime responseTime, ApplicationServer applicationServer, string comment)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            this.PingRequestId       = pingRequestId;
            this.ApplicationServer   = applicationServer;
            this.ApplicationServerId = applicationServer.Id;
            this.ResponseTime        = responseTime;
            this.Comment             = comment;
        }
    }
}
