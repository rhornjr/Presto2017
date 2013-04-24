using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
    #pragma warning disable 1591
    [DataContract]
    public enum InstallationResult
    {
        [DataMember] Unknown,
        [DataMember] Success,
        [DataMember] PartialSuccess, // At least one task succeeded
        [DataMember] Failure  // No tasks succeeded
    }
    #pragma warning restore 1591
}
