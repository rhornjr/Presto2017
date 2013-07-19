using System.Collections.Generic;
using System.Runtime.Serialization;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class AdGroupWithRoles : EntityBase
    {
        [DataMember]
        public string AdGroupName { get; set; }

        [DataMember]
        public List<PrestoRole> PrestoRoles { get; set; }
    }
}
