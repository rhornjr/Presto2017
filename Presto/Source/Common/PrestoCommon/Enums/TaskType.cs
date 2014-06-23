
using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
    #pragma warning disable 1591
    [DataContract]
    public enum TaskType
    {
        [EnumMember] CopyFile,
        [EnumMember] DosCommand,        
        [EnumMember] XmlModify,
        [EnumMember] VersionChecker,
        [EnumMember] App
        //Installer  // Will enable this if we ever need it again.
    }
    #pragma warning restore 1591
}
