using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PrestoCommon.Enums
{
    #pragma warning disable 1591
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))] // So angular can show the string value on the web page
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
