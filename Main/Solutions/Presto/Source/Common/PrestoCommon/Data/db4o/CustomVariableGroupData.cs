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
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByApplicationName(string applicationName)
        {
            CustomVariableGroup group = (from CustomVariableGroup customGroup in Database
                                         where customGroup.Application != null && customGroup.Application.Name == applicationName
                                         select customGroup).FirstOrDefault();

            if (group == null) { return null; }

            Database.Ext().Refresh(group, 10);

            return group;
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public List<CustomVariableGroup> GetByIds(List<string> ids)
        {
            // Only RavenDB needs this.
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public CustomVariableGroup GetById(string id)
        {
            // Only RavenDB needs this.
            throw new System.NotImplementedException();
        }
    }
}
