using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
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
            IEnumerable<CustomVariableGroup> groups = from CustomVariableGroup customGroup in Database
                                                      select customGroup;

            Database.Ext().Refresh(groups, 10);

            return groups;
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByName(string name)
        {
            CustomVariableGroup group = (from CustomVariableGroup customGroup in Database
                                         where customGroup.Name == name
                                         select customGroup).FirstOrDefault();

            if (group == null) { return null; }

            Database.Ext().Refresh(group, 10);

            return group;
        }

        /// <summary>
        /// Gets the specified application name.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByApplication(Application application)
        {
            CustomVariableGroup group = (from CustomVariableGroup customGroup in Database
                                         where customGroup.Application != null && customGroup.Application.Name == application.Name
                                         select customGroup).FirstOrDefault();

            if (group == null) { return null; }

            Database.Ext().Refresh(group, 10);

            return group;
        }

        /// <summary>
        /// Saves the specified custom variable group.
        /// </summary>
        /// <param name="customVariableGroup">The custom variable group.</param>
        public void Save(CustomVariableGroup customVariableGroup)
        {
            new GenericData().Save(customVariableGroup);
        }
    }
}
