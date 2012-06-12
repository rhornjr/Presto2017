using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client.Linq;

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
            return ExecuteQuery<IEnumerable<CustomVariableGroup>>(() =>
            {
                IEnumerable<CustomVariableGroup> customGroups =
                    QueryAndSetEtags(session =>
                        session.Query<CustomVariableGroup>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .Take(int.MaxValue))
                        .AsEnumerable().Cast<CustomVariableGroup>();

                return customGroups;
            });
        }        

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByName(string name)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup =
                    QuerySingleResultAndSetEtag(session => session.Query<CustomVariableGroup>()
                        .Where(customGroup => customGroup.Name == name).FirstOrDefault())
                        as CustomVariableGroup;

                return customVariableGroup;
            });
        }

        /// <summary>
        /// Gets the object by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public CustomVariableGroup GetById(string id)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup =
                    QuerySingleResultAndSetEtag(session => session.Query<CustomVariableGroup>()
                        .Where(customGroup => customGroup.Id == id).FirstOrDefault())
                        as CustomVariableGroup;

                return customVariableGroup;
            });
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
