using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableGroupLogic : LogicBase
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CustomVariableGroup> GetAll()
        {
            IEnumerable<CustomVariableGroup> groups = from CustomVariableGroup customGroup in Database
                                                      select customGroup;

            Database.Ext().Refresh(groups, 10);

            return groups;
        }

        /// <summary>
        /// Gets the CustomVariableGroup by the specified application name.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public static CustomVariableGroup Get(string applicationName)
        {
            CustomVariableGroup group = (from CustomVariableGroup customGroup in Database
                                         where customGroup.Application != null && customGroup.Application.Name == applicationName
                                         select customGroup).FirstOrDefault();

            if (group == null) { return null; }

            Database.Ext().Refresh(group, 10);

            return group;
        }
    }
}
