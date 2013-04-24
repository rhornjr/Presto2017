using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class InstallationEnvironment : EntityBase
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int LogicalOrder { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
