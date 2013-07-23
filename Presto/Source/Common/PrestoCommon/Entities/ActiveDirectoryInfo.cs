using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class ActiveDirectoryInfo : EntityBase
    {
        [DataMember]
        public bool SecurityEnabled { get; set; }

        [DataMember]
        public string Domain { get; set; }

        [DataMember]
        public string DomainSuffix { get; set; }

        [DataMember]
        public string DomainPort { get; set; }

        [DataMember]
        public string ActiveDirectoryAccountUser { get; set; }

        [DataMember]
        public string ActiveDirectoryAccountPassword { get; set; }
    }
}
