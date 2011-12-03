
namespace PrestoCommon.Enums
{
    /// <summary>
    /// The status of an installation
    /// </summary>
    #pragma warning disable 1591
    public enum InstallationResult
    {
        Unknown,
        Success,
        PartialSuccess, // At least one task succeeded
        Failure  // No tasks succeeded
    }
    #pragma warning restore 1591
}
