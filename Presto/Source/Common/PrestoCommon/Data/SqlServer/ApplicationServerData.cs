using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        public IEnumerable<ApplicationServer> GetAll()
        {
            return this.Database.ApplicationServers
                .Include(x => x.ApplicationsWithOverrideGroup.Select(y => y.Application))
                .Include(x => x.ApplicationsWithOverrideGroup.Select(y => y.CustomVariableGroup))
                .Include(x => x.ApplicationWithGroupToForceInstallList)
                .Include(x => x.CustomVariableGroups);
        }

        public ApplicationServer GetByName(string serverName)
        {
            return this.Database.ApplicationServers
                .Include(x => x.ApplicationsWithOverrideGroup)
                .Include(x => x.ApplicationWithGroupToForceInstallList)
                .Include(x => x.CustomVariableGroups)
                .Where(x => x.Name == serverName)
                .FirstOrDefault();
        }

        public ApplicationServer GetById(string id)
        {
            int idAsInt = Convert.ToInt32(id, CultureInfo.InvariantCulture);

            return this.Database.ApplicationServers
                .Include(x => x.ApplicationsWithOverrideGroup)
                .Include(x => x.ApplicationWithGroupToForceInstallList)
                .Include(x => x.CustomVariableGroups)
                .Where(x => x.IdForEf == idAsInt)
                .FirstOrDefault();
        }

        public void Save(ApplicationServer newServer)
        {
            if (newServer == null) { throw new ArgumentNullException("newServer"); }

            // ToDo: Save this too: ApplicationWithGroupToForceInstallList

            int[] groupIds = GetGroupIds(newServer.CustomVariableGroups.ToList());

            ApplicationServer serverFromContext;

            if (newServer.IdForEf == 0)  // New app
            {
                serverFromContext = newServer;
                // Clear groups, otherwise new groups will be added to the groups table.
                serverFromContext.ApplicationsWithOverrideGroup.Clear();
                serverFromContext.CustomVariableGroups.Clear();
                this.Database.ApplicationServers.Add(serverFromContext);
            }
            else
            {
                serverFromContext = this.Database.ApplicationServers
                    .Include(x => x.ApplicationsWithOverrideGroup)
                    .Include(x => x.CustomVariableGroups)
                    .Single(x => x.IdForEf == newServer.IdForEf);
            }

            AddAppGroupsToServer(newServer, serverFromContext);
            AddGroupsToApp(groupIds, serverFromContext);
            this.Database.SaveChanges();
        }

        private void AddAppGroupsToServer(ApplicationServer serverNotAssociatedWithContext, ApplicationServer serverFromContext)
        {
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in serverNotAssociatedWithContext.ApplicationsWithOverrideGroup)
            {
                if (appWithGroup.IdForEf == 0)
                {
                    var emptyAppWithGroup = new ApplicationWithOverrideVariableGroup() { Enabled = appWithGroup.Enabled };
                    // Add with no app or group so new app or group doesn't get added to DB
                    serverFromContext.ApplicationsWithOverrideGroup.Add(emptyAppWithGroup);

                    // Add app
                    Application app = this.Database.Applications.Single(x => x.IdForEf == appWithGroup.Application.IdForEf);
                    emptyAppWithGroup.Application = app;
                    
                    if (appWithGroup.CustomVariableGroup == null) { continue; }

                    // Add group
                    CustomVariableGroup group = this.Database.CustomVariableGroups.Single(x => x.IdForEf == appWithGroup.CustomVariableGroup.IdForEf);
                    emptyAppWithGroup.CustomVariableGroup = group;
                }
                else
                {
                    ApplicationWithOverrideVariableGroup appGroup =
                        serverFromContext.ApplicationsWithOverrideGroup.Single(x => x.IdForEf == appWithGroup.IdForEf);  // Get original
                    this.Database.Entry(appGroup).CurrentValues.SetValues(appWithGroup);  // Update with new
                }
            }

            // Delete items that no longer exist within the app.
            List<ApplicationWithOverrideVariableGroup> appGroupsToDelete = new List<ApplicationWithOverrideVariableGroup>();
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in serverFromContext.ApplicationsWithOverrideGroup)
            {
                ApplicationWithOverrideVariableGroup appGroupInNewServer =
                    serverNotAssociatedWithContext.ApplicationsWithOverrideGroup.Where(x => x.IdForEf == appWithGroup.IdForEf).FirstOrDefault();

                if (appGroupInNewServer == null)
                {
                    appGroupsToDelete.Add(appWithGroup);
                }
            }

            foreach (ApplicationWithOverrideVariableGroup appGroupToDelete in appGroupsToDelete)
            {
                serverFromContext.ApplicationsWithOverrideGroup.Remove(appGroupToDelete);
                this.Database.ApplicationWithOverrideVariableGroups.Remove(appGroupToDelete);
            }
        }

        private void AddGroupsToApp(int[] groupIds, ApplicationServer server)
        {
            server.CustomVariableGroups.Clear();

            List<CustomVariableGroup> groupsFromDb =
                this.Database.CustomVariableGroups.Where(g => groupIds.Contains(g.IdForEf)).ToList();

            foreach (CustomVariableGroup group in groupsFromDb)
            {
                server.CustomVariableGroups.Add(group);
            }
        }
    }
}
