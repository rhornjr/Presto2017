using System;
using System.Runtime.Serialization;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class PingResponse : EntityBase
    {
        [DataMember]
        public string PingRequestId { get; private set; }

        [DataMember]
        public string ApplicationServerId { get; set; }

        [DataMember]
        public DateTime ResponseTime { get; private set; }

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public ApplicationServer ApplicationServer { get; set; }

        [DataMember]
        public string Comment { get; set; }

        public PingResponse() { }

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
