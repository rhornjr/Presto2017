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
                    QueryAndCacheEtags(session => session.Advanced.LuceneQuery<CustomVariableGroup>()
                    .Include(x => x.ApplicationId)).Cast<CustomVariableGroup>();

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

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public CustomVariableGroup GetById(string id)
        {
            return ExecuteQuery<CustomVariableGroup>(() =>
            {
                CustomVariableGroup customVariableGroup = 
                    QuerySingleResultAndCacheEtag(session => session.Query<CustomVariableGroup>()
                        .Include(x => x.ApplicationId)
                        .Where(group => group.Id == id).FirstOrDefault()) as CustomVariableGroup;

                HydrateApplication(customVariableGroup);

                return customVariableGroup;
            });
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <returns></returns>
        public IEnumerable<CustomVariableGroup> GetByIds(IEnumerable<string> groupIds)
        {
            return ExecuteQuery<IEnumerable<CustomVariableGroup>>(() =>
            {
                IEnumerable<CustomVariableGroup> customGroups = QueryAndCacheEtags(
                    session => session.Query<CustomVariableGroup>()
                        .Include(x => x.ApplicationId)
                        .Where(group => group.Id.In<string>(groupIds))).Cast<CustomVariableGroup>();

                foreach (CustomVariableGroup customGroup in customGroups)
                {
                    HydrateApplication(customGroup);
                }

                return customGroups;
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
