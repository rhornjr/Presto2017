using System;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Common entity behavior.
    /// </summary>
    public class EntityBase : NotifyPropertyChangedBase
    {
        // Did this now so we don't have to change the inheritance
        // hierarchy later.

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public string Id { get; set; }  // Required field for all objects with RavenDB.

        /// <summary>
        /// Raven's version of VersionNumber. Used for optimistic concurrency.
        /// </summary>
        public Guid Etag { get; set; }
    }
}
