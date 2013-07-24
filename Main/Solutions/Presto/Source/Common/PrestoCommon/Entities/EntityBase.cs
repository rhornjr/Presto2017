using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using Xanico.Core.Utilities;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Common entity behavior.
    /// </summary>    
    [DataContract]
    [KnownType("DerivedTypes")]
    public class EntityBase : NotifyPropertyChangedBase
    {
        [DataMember]
        public string Id { get; set; }  // Required field for all objects with RavenDB.

        [DataMember]
        public Guid Etag { get; set; }  // For RavenDB

        private static Type[] DerivedTypes()
        {
            return typeof(EntityBase).GetDerivedTypes(Assembly.GetExecutingAssembly()).ToArray();
        }
    }
}
