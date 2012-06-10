using System;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class PingRequest : EntityBase
    {
        /// <summary>
        /// Gets the request time.
        /// </summary>
        public DateTime RequestTime { get; private set; }

        /// <summary>
        /// Gets the user initiating request.
        /// </summary>
        public string UserInitiatingRequest { get; private set; }

        public PingRequest()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PingRequest"/> class.
        /// </summary>
        /// <param name="requestTime">The request time.</param>
        /// <param name="user">The user.</param>
        public PingRequest(DateTime requestTime, string user)
        {
            this.RequestTime           = requestTime;
            this.UserInitiatingRequest = user;
        }
    }
}
