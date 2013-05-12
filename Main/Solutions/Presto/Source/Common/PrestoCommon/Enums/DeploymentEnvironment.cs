// ToDo: Remove this after migrating the data to InstallationEnvironment

using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
#pragma warning disable 1591
    [DataContract]
    public enum DeploymentEnvironment
    {
        [EnumMember] Unknown,
        [EnumMember] Development,
        [EnumMember] QA,
        [EnumMember] Production
    }
#pragma warning restore 1591
}
