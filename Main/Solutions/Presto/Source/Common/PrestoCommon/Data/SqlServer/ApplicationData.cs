using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class ApplicationData : DataAccessLayerBase, IApplicationData
    {
        public IEnumerable<Application> GetAll()
        {
            return this.Database.Applications
                .Include(x => x.Tasks)
                .Include(x => x.CustomVariableGroups)
                .ToList();
        }

        public Application GetByName(string name)
        {
            return this.Database.Applications
                .Include(x => x.Tasks)
                .Include(x => x.CustomVariableGroups)
                .Where(x => x.Name == name)
                .FirstOrDefault();
        }

        public Application GetById(string id)
        {
            int idAsInt = Convert.ToInt32(id, CultureInfo.InvariantCulture);

            return this.Database.Applications
                .Include(x => x.Tasks)
                .Include(x => x.CustomVariableGroups)
                .Where(x => x.IdForEf == idAsInt)
                .FirstOrDefault();
        }

        public void Save(Application newApp)
        {
            if (newApp == null) { throw new ArgumentNullException("newApp"); }

            int[] groupIds = GetGroupIds(newApp.CustomVariableGroups.ToList());

            Application appFromContext;

            if (newApp.IdForEf == 0)  // New app
            {
                appFromContext = newApp;
                // Clear groups, otherwise new groups will be added to the groups table.
                appFromContext.CustomVariableGroups.Clear();
                this.Database.Applications.Add(appFromContext);                
            }
            else
            {
                appFromContext = this.Database.Applications
                    .Include(x => x.Tasks)
                    .Include(x => x.CustomVariableGroups)
                    .Single(x => x.IdForEf == newApp.IdForEf);
            }

            AddTasksToApp(newApp, appFromContext);
            AddGroupsToApp(groupIds, appFromContext);
            this.Database.SaveChanges();
        }

        private void AddTasksToApp(Application appNotAssociatedWithContext, Application appFromContext)
        {
            foreach (TaskBase taskModified in appNotAssociatedWithContext.Tasks)
            {
                if (taskModified.IdForEf == 0)
                {
                    appFromContext.Tasks.Add(taskModified);
                }
                else
                {
                    TaskBase taskBase = appFromContext.Tasks.Single(x => x.IdForEf == taskModified.IdForEf);  // Get original task
                    this.Database.Entry(taskBase).CurrentValues.SetValues(taskModified);  // Update with new
                }
            }

            // Delete tasks that no longer exist within the app.
            List<TaskBase> tasksToDelete = new List<TaskBase>();
            foreach (TaskBase originalTask in appFromContext.Tasks)
            {
                TaskBase task = appNotAssociatedWithContext.Tasks.Where(x => x.IdForEf == originalTask.IdForEf).FirstOrDefault();

                if (task == null)
                {                    
                    tasksToDelete.Add(originalTask);
                }
            }

            foreach (TaskBase taskToDelete in tasksToDelete)
            {                
                appFromContext.Tasks.Remove(taskToDelete);
                this.Database.TaskBases.Remove(taskToDelete);
            }
        }

        private void AddGroupsToApp(int[] groupIds, Application app)
        {
            app.CustomVariableGroups.Clear();

            List<CustomVariableGroup> groupsFromDb =
                this.Database.CustomVariableGroups.Where(g => groupIds.Contains(g.IdForEf)).ToList();

            foreach (CustomVariableGroup group in groupsFromDb)
            {
                app.CustomVariableGroups.Add(group);
            }
        }
    }
}
