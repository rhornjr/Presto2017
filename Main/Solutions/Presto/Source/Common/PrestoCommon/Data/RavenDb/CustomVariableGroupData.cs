using System;
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
                    QueryAndCacheEtags(session => session.Query<CustomVariableGroup>()
                    .Include(x => x.ApplicationId)).AsEnumerable().Cast<CustomVariableGroup>();

                foreach (CustomVariableGroup customGroup in customGroups)
                {
                    HydrateApplication(customGroup);
                }

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
                    QuerySingleResultAndCacheEtag(session => session.Query<CustomVariableGroup>()
                        .Include(x => x.ApplicationId)
                        .Where(customGroup => customGroup.Name == name).FirstOrDefault())
                        as CustomVariableGroup;

                HydrateApplication(customVariableGroup);

                return customVariableGroup;
            });
        }        

        /// <summary>
        /// Gets the specified application name.
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public CustomVariableGroup GetByApplication(Application application)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup = 
                    QuerySingleResultAndCacheEtag(session => session.Query<CustomVariableGroup>()
                        .Include(x => x.ApplicationId)
                        .Where(customGroup => customGroup.ApplicationId == application.Id).FirstOrDefault())
                        as CustomVariableGroup;

                HydrateApplication(customVariableGroup);

                return customVariableGroup;
            });
        }

        private static void HydrateApplication(CustomVariableGroup customVariableGroup)
        {
            if (customVariableGroup == null) { return; }

            if (!string.IsNullOrWhiteSpace(customVariableGroup.ApplicationId))
            {
                customVariableGroup.Application = QuerySingleResultAndCacheEtag(session => session.Load<Application>(customVariableGroup.ApplicationId)) as Application;
            }
        }

        /// <summary>
        /// Saves the specified custom variable group.
        /// </summary>
        /// <param name="customVariableGroup">The custom variable group.</param>
        public void Save(CustomVariableGroup customVariableGroup)
        {
            if (customVariableGroup == null) { throw new ArgumentNullException("customVariableGroup"); }

            if (customVariableGroup.Application != null)
            {
                customVariableGroup.ApplicationId = customVariableGroup.Application.Id;
            }

            new GenericData().Save(customVariableGroup);
        }
    }
}
