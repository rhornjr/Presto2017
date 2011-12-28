using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
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
            ApplicationWithOverrideVariableGroup appOverrideGroup =
                (from ApplicationWithOverrideVariableGroup appGroup in Database
                 where appGroup.Application.Name.ToUpperInvariant() == appName.ToUpperInvariant()
                    && appGroup.CustomVariableGroup.Name.ToUpperInvariant() == groupName.ToUpperInvariant()
                 select appGroup).FirstOrDefault();

            Database.Ext().Refresh(appOverrideGroup, 10);

            return appOverrideGroup;
        }
    }
}
