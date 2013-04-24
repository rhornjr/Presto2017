
using System.Runtime.Serialization;

namespace PrestoCommon.Enums
{
    #pragma warning disable 1591
    [DataContract]
    public enum TaskType
    {
        [DataMember] CopyFile,
        [DataMember] DosCommand,        
        [DataMember] XmlModify,
        [DataMember] VersionChecker
        //Installer  // Will enable this if we ever need it again.
    }
    #pragma warning restore 1591
}
