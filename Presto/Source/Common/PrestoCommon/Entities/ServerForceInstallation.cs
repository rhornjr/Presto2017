using System.Collections.Generic;
using System.Runtime.Serialization;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class ServerForceInstallation : EntityBase
    {
        [DataMember]
        public string ApplicationServerId { get; set; }

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public ApplicationServer ApplicationServer { get; set; }

        [DataMember]
        public string ApplicationId { get; set; }

        [DataMember]
        public List<string> OverrideGroupIds { get; set; }

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public ApplicationWithOverrideVariableGroup ApplicationWithOverrideGroup { get; set; }

        public ServerForceInstallation(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            this.ApplicationServer            = server;
            this.ApplicationWithOverrideGroup = appWithGroup;
        }
    }
}
