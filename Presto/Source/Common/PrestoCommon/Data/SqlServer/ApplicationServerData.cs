using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                .Include(x => x.ApplicationsWithOverrideGroup)
                .Include(x => x.ApplicationWithGroupToForceInstallList)
                .Include(x => x.CustomVariableGroups)
                .ToList();
        }

        public ApplicationServer GetByName(string serverName)
        {
            throw new NotImplementedException();
        }

        public ApplicationServer GetById(string id)
        {
            throw new NotImplementedException();
        }

        public void Save(ApplicationServer newServer)
        {
            if (newServer == null) { throw new ArgumentNullException("newServer"); }

            int[] groupIds = GetGroupIds(newServer.CustomVariableGroups.ToList());

            ApplicationServer serverFromContext;

            if (newServer.IdForEf == 0)  // New app
            {
                serverFromContext = newServer;
                // Clear groups, otherwise new groups will be added to the groups table.
                serverFromContext.CustomVariableGroups.Clear();
                this.Database.ApplicationServers.Add(serverFromContext);
            }
            else
            {
                serverFromContext = this.Database.ApplicationServers
                    .Include(x => x.CustomVariableGroups)
                    .Single(x => x.IdForEf == newServer.IdForEf);
            }

            AddGroupsToApp(groupIds, serverFromContext);
            this.Database.SaveChanges();
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
