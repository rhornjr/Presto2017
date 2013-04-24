using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class PingRequest : EntityBase
    {
        [DataMember]
        public DateTime RequestTime { get; private set; }

        [DataMember]
        public string UserInitiatingRequest { get; private set; }

        public PingRequest(DateTime requestTime, string user)
        {
            this.RequestTime           = requestTime;
            this.UserInitiatingRequest = user;
        }
    }
}
