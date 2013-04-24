using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Common entity behavior.
    /// </summary>
    [DataContract]
    public class EntityBase : NotifyPropertyChangedBase
    {
        [DataMember]
        public string Id { get; set; }  // Required field for all objects with RavenDB.

        [DataMember]
        public Guid Etag { get; set; }  // For RavenDB
    }
}
