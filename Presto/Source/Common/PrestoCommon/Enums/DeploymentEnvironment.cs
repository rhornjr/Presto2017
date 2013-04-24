// ToDo: Remove this after migrating the data to InstallationEnvironment

using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
#pragma warning disable 1591
    [DataContract]
    public enum DeploymentEnvironment
    {
        [DataMember] Unknown,
        [DataMember] Development,
        [DataMember] QA,
        [DataMember] Production
    }
#pragma warning restore 1591
}
