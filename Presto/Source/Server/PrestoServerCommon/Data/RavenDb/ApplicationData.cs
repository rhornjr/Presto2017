using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoServer.Data.Interfaces;
using Raven.Client;
using Raven.Client.Linq;

namespace PrestoServer.Data.RavenDb
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
        public IEnumerable<Application> GetAll(bool includeArchivedApps)
        {
            return ExecuteQuery<IEnumerable<Application>>(() =>
            {
                IEnumerable<Application> apps = QueryAndSetEtags(session =>
                    session.Query<Application>()
                    .Include(x => x.CustomVariableGroupIds)
                    .Include(x => x.CustomVariableGroupIdsForTaskApps)
                    .Take(int.MaxValue))
                    .AsEnumerable().Cast<Application>();

                if (includeArchivedApps)
                {
                    foreach (Application app in apps) { HydrateApplication(app); }
                    return apps;
                }

                // Note: We can't put this WHERE clause in the above query because the Archived
                //       property was added after all of the other properties. It won't exist
                //       for all apps, so we can't query by that property.
                //       http://stackoverflow.com/a/11644645/279516
                var appsNotArchived = apps.Where(x => x.Archived != true);
                foreach (Application app in appsNotArchived) { HydrateApplication(app); }
                return appsNotArchived;
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
                    .Include(x => x.CustomVariableGroupIdsForTaskApps)
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
                    session.Include<Application>(
                        x => x.CustomVariableGroupIds).Include(x => x.CustomVariableGroupIdsForTaskApps)
                    .Load<Application>(id))
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

            foreach (var task in app.Tasks)
            {
                var taskApp = task as TaskApp;

                if (taskApp == null) { continue; }

                taskApp.AppWithGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();

                if (taskApp.AppWithGroup.CustomVariableGroupIds != null)
                {
                    foreach (string cvgId in taskApp.AppWithGroup.CustomVariableGroupIds)
                    {
                        taskApp.AppWithGroup.CustomVariableGroups.Add
                            (QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(cvgId)) as CustomVariableGroup);
                    }
                }

                // CustomVariableGroupId can still exist for older tasks, as long as they haven't been loaded and saved again.
                string oldCvgId = taskApp.AppWithGroup.CustomVariableGroupId;
                if (oldCvgId != null)
                {
                    // Add the CVG if it doesn't already exist.
                    if (!taskApp.AppWithGroup.CustomVariableGroups.Select(x => x.Id).Contains(oldCvgId))
                    {
                        taskApp.AppWithGroup.CustomVariableGroups.Add
                            (QuerySingleResultAndSetEtag(session => session.Load<CustomVariableGroup>(oldCvgId)) as CustomVariableGroup);
                    }
                    taskApp.AppWithGroup.CustomVariableGroupId = null;  // Set the old property to null. Eventually they'll all be this way.
                }
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

            if (application.CustomVariableGroups != null && application.CustomVariableGroups.Count > 0)
            {
                foreach (CustomVariableGroup group in application.CustomVariableGroups)
                {
                    application.CustomVariableGroupIds.Add(group.Id);
                }
            }

            // For each task in an application, that is of type TaskApp, set its CVG IDs.
            application.CustomVariableGroupIdsForTaskApps = new List<string>();
            foreach (var task in application.Tasks)
            {
                var taskApp = task as TaskApp;

                if (taskApp == null) { continue; }

                taskApp.AppWithGroup.CustomVariableGroupIds = new List<string>();

                if (taskApp.AppWithGroup.CustomVariableGroups == null) { continue; }

                foreach (var cvg in taskApp.AppWithGroup.CustomVariableGroups)
                {
                    taskApp.AppWithGroup.CustomVariableGroupIds.Add(cvg.Id);
                    application.CustomVariableGroupIdsForTaskApps.Add(cvg.Id);
                }
            }
        }
    }
}
