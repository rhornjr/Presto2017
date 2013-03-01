
namespace PrestoCommon.Enums
{
    /// <summary>
    /// Task type
    /// </summary>
    #pragma warning disable 1591
    public enum TaskType
    {
        CopyFile,
        DosCommand,        
        XmlModify,
        VersionChecker
        //Installer  // Will enable this if we ever need it again.
    }
    #pragma warning restore 1591
}
