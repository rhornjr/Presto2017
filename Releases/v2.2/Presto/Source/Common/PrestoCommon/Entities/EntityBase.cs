using System;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Common entity behavior.
    /// </summary>
    public class EntityBase : NotifyPropertyChangedBase
    {
        public string Id { get; set; }  // Required field for all objects with RavenDB.
        public Guid Etag { get; set; }  // For RavenDB
    }
}
