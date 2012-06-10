using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class InstallationSummaryData : DataAccessLayerBase, IInstallationSummaryData
    {
        public IEnumerable<InstallationSummary> GetByServerAppAndGroup(ApplicationServer appServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            IQueryable<InstallationSummary> summaries = this.Database.InstallationSummaries
                .Include(x => x.ApplicationServer)
                .Include(x => x.ApplicationWithOverrideVariableGroup.Application)
                .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroup)
                .Where(x => x.ApplicationServer.IdForEf == appServer.IdForEf)
                .Where(x => x.ApplicationWithOverrideVariableGroup.Application.IdForEf == appWithGroup.Application.IdForEf);

            if (appWithGroup.CustomVariableGroup == null)
            {
                return summaries.Where(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroup == null);
            }

            List<InstallationSummary> summaryList = summaries
                .Where(x =>
                x.ApplicationWithOverrideVariableGroup != null &&
                x.ApplicationWithOverrideVariableGroup.CustomVariableGroup != null &&
                x.ApplicationWithOverrideVariableGroup.CustomVariableGroup.IdForEf == appWithGroup.CustomVariableGroup.IdForEf).ToList();

            return summaryList;
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return this.Database.InstallationSummaries
                .Include(x => x.ApplicationServer)
                .Include(x => x.ApplicationWithOverrideVariableGroup.Application)
                .Include(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroup)
                .OrderByDescending(x => x.InstallationStart)
                .Take(numberToRetrieve);
        }

        public void Save(InstallationSummary newInstallationSummary)
        {
            if (newInstallationSummary == null) { throw new ArgumentNullException("newInstallationSummary"); }

            // ToDo: Save child/list properties:
            // + ApplicationServer
            // + ApplicationWithOverrideVariableGroup
            // - InstallationResult
            // - TaskDetails

            // We only add new InstallationSummary objects. We never modify.

            // Primitive types can be included when the summary is initially added to the DB.
            InstallationSummary installationSummaryToSave = new InstallationSummary();
            installationSummaryToSave.InstallationEnd     = newInstallationSummary.InstallationEnd;
            installationSummaryToSave.InstallationResult  = newInstallationSummary.InstallationResult;            
            installationSummaryToSave.InstallationStart   = newInstallationSummary.InstallationStart;
            installationSummaryToSave.TaskDetails         = newInstallationSummary.TaskDetails;

            if (newInstallationSummary.ApplicationWithOverrideVariableGroup.IdForEf == 0)
            {
                ApplicationWithOverrideVariableGroup newAppWithGroup = new ApplicationWithOverrideVariableGroup();
                newAppWithGroup.Enabled = newInstallationSummary.ApplicationWithOverrideVariableGroup.Enabled;
                installationSummaryToSave.ApplicationWithOverrideVariableGroup = newAppWithGroup;
            }

            this.Database.InstallationSummaries.Add(installationSummaryToSave);

            // Now add the complex types
            if (newInstallationSummary.ApplicationWithOverrideVariableGroup.IdForEf == 0)
            {
                ApplicationServer server = this.Database.ApplicationServers.Single(x => x.IdForEf == newInstallationSummary.ApplicationServer.IdForEf);
                installationSummaryToSave.ApplicationServer = server;

                server.ApplicationsWithOverrideGroup.Add(installationSummaryToSave.ApplicationWithOverrideVariableGroup);

                Application app = this.Database.Applications.Single(x => x.IdForEf == newInstallationSummary.ApplicationWithOverrideVariableGroup.Application.IdForEf);
                installationSummaryToSave.ApplicationWithOverrideVariableGroup.Application = app;

                if (newInstallationSummary.ApplicationWithOverrideVariableGroup.CustomVariableGroup != null)
                {
                    CustomVariableGroup group = this.Database.CustomVariableGroups.Single(x => x.IdForEf == newInstallationSummary.ApplicationWithOverrideVariableGroup.CustomVariableGroup.IdForEf);
                    installationSummaryToSave.ApplicationWithOverrideVariableGroup.CustomVariableGroup = group;
                }
            }
            else
            {
                ApplicationWithOverrideVariableGroup appWithGroup =
                    this.Database.ApplicationWithOverrideVariableGroups.Single(x => x.IdForEf == newInstallationSummary.ApplicationWithOverrideVariableGroup.IdForEf);
                installationSummaryToSave.ApplicationWithOverrideVariableGroup = appWithGroup;
            }            

            this.Database.SaveChanges();
        }
    }
}
