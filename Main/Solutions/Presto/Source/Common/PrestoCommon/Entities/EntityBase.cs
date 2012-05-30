using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Common entity behavior.
    /// </summary>
    public class EntityBase : NotifyPropertyChangedBase
    {
        [NotMapped]  // RavenDB only
        public string Id { get; set; }  // Required field for all objects with RavenDB.

        public Guid Etag { get; set; }  // For RavenDB

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ef")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ef")]
        [Key]
        [JsonIgnore]  // Not for RavenDB
        public int IdForEf
        {
            get { return Convert.ToInt32(this.Id, CultureInfo.InvariantCulture); }

            set { this.Id = value.ToString(CultureInfo.InvariantCulture); }
        }
    }
}
