using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
    #pragma warning disable 1591
    [DataContract]
    public enum InstallationResult
    {
        [EnumMember] Unknown,
        [EnumMember] Success,
        [EnumMember] PartialSuccess, // At least one task succeeded
        [EnumMember] Failure  // No tasks succeeded
    }
    #pragma warning restore 1591
}
