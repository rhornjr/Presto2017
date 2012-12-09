using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client.Linq;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationData : DataAccessLayerBase, IApplicationData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Application> GetAll()
        {
            return ExecuteQuery<IEnumerable<Application>>(() =>
            {
                IEnumerable<Application> apps = QueryAndSetEtags(session =>
                    session.Query<Application>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Take(int.MaxValue))
                    .AsEnumerable().Cast<Application>();

                foreach (Application app in apps) { HydrateApplication(app);                }

                return apps;
            });
        }        

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Application GetByName(string name)
        {
            return ExecuteQuery<Application>(() =>
            {
                Application application = QuerySingleResultAndSetEtag(session =>
                    session.Query<Application>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Where(app => app.Name == name).FirstOrDefault())
                    as Application;

                HydrateApplication(application);

                return application;
            });
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Application GetById(string id)
        {
            return ExecuteQuery<Application>(() =>
            {
                Application application = QuerySingleResultAndSetEtag(session =>
                    session.Query<Application>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Where(app => app.Id == id).FirstOrDefault())
                    as Application;

                HydrateApplication(application);

                return application;
            });
        }

        /// <summary>
        /// Hydrates the application.
        /// </summary>
        /// <param name="app">The app.</param>
        public static void HydrateApplication(Application app)
        {
            if (app == null) { throw new ArgumentNullException("app"); }
            if (app.CustomVariableGroupIds == null) { return; }

            app.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>();

            foreach (string groupId in app.CustomVariableGroupIds)
            {                
                app.CustomVariableGroups.Add(QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(groupId)) as CustomVariableGroup);
            }
        }

        /// <summary>
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Save(Application application)
        {
            if (application == null) { throw new ArgumentNullException("application"); }

            SetCustomVariableGroupIds(application);

            new GenericData().Save(application);
        }

        private static void SetCustomVariableGroupIds(Application application)
        {
            application.CustomVariableGroupIds = new List<string>();

            if (application.CustomVariableGroups == null || application.CustomVariableGroups.Count < 1) { return; }

            foreach (CustomVariableGroup group in application.CustomVariableGroups)
            {
                application.CustomVariableGroupIds.Add(group.Id);
            }
        }
    }
}
