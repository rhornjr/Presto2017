﻿using System.Collections.Generic;
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
            return QueryAndCacheEtags(session => session.Query<CustomVariableGroup>()).Cast<CustomVariableGroup>();
        }        

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByName(string name)
        {
            return QuerySingleResultAndCacheEtag(session => session.Query<CustomVariableGroup>()
                .Where(customGroup => customGroup.Name == name).FirstOrDefault())
                as CustomVariableGroup;
        }

        /// <summary>
        /// Gets the specified application name.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public CustomVariableGroup GetByApplicationName(string applicationName)
        {
            return QuerySingleResultAndCacheEtag(session => session.Query<CustomVariableGroup>()
                .Where(customGroup => customGroup.Application != null && customGroup.Application.Name == applicationName).FirstOrDefault())
                as CustomVariableGroup;
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public CustomVariableGroup GetById(string id)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<CustomVariableGroup>().Where(group => group.Id == id).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public List<CustomVariableGroup> GetByIds(List<string> ids)
        {
            List<CustomVariableGroup> customVariableGroups = new List<CustomVariableGroup>();

            if (ids == null) { return customVariableGroups; }            

            using (IDocumentSession session = Database.OpenSession())
            {
                foreach (string id in ids)
                {
                    customVariableGroups.Add(session.Query<CustomVariableGroup>()
                        .Where(group => group.Id == id)
                        .FirstOrDefault());
                }
            }

            return customVariableGroups;
        }
    }
}
