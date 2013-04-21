using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    public class ServerForceInstallation : EntityBase
    {
        public string ApplicationServerId { get; set; }

        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public ApplicationServer ApplicationServer { get; set; }

        public string ApplicationId { get; set; }

        public string OverrideGroupId { get; set; }

        [JsonIgnore]  //  We do not want RavenDB to serialize this.
        public ApplicationWithOverrideVariableGroup ApplicationWithOverrideGroup { get; set; }

        public ServerForceInstallation(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            this.ApplicationServer            = server;
            this.ApplicationWithOverrideGroup = appWithGroup;
        }
    }
}
