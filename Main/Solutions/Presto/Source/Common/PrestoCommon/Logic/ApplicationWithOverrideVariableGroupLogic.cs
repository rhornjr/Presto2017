using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationWithOverrideVariableGroupLogic : LogicBase
    {
        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        public static ApplicationWithOverrideVariableGroup GetByAppNameAndGroupName(string appName, string groupName)
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
