using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableGroupData : DataAccessLayerBase, ICustomVariableGroupData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CustomVariableGroup> GetAll()
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<CustomVariableGroup>();
            }
        }

        /// <summary>
        /// Gets the specified application name.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByName(string applicationName)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<CustomVariableGroup>()
                    .Where(customGroup => customGroup.Application != null && customGroup.Application.Name == applicationName).FirstOrDefault();
            }
        }
    }
}
