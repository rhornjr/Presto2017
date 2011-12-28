using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationWithOverrideVariableGroupData : DataAccessLayerBase, IApplicationWithOverrideVariableGroupData
    {
        /// <summary>
        /// Gets the name of the by app name and group.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        public ApplicationWithOverrideVariableGroup GetByAppNameAndGroupName(string appName, string groupName)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<ApplicationWithOverrideVariableGroup>()
                    .Where(appGroup => appGroup.Application.Name.ToUpperInvariant() == appName.ToUpperInvariant()
                        && appGroup.CustomVariableGroup.Name.ToUpperInvariant() == groupName.ToUpperInvariant()).FirstOrDefault();
            }
        }
    }
}
