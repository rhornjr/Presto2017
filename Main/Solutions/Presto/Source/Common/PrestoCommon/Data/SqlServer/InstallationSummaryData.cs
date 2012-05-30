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
                .Include(x => x.ApplicationWithOverrideVariableGroup)
                .Where(x => x.ApplicationServer.IdForEf == appServer.IdForEf)
                .Where(x => x.ApplicationWithOverrideVariableGroup.IdForEf == appWithGroup.Application.IdForEf);

            if (appWithGroup.CustomVariableGroup == null)
            {
                return summaries.Where(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupId == null);
            }

            return summaries
                .Where(x =>
                x.ApplicationWithOverrideVariableGroup != null &&
                x.ApplicationWithOverrideVariableGroup.CustomVariableGroup != null &&
                x.ApplicationWithOverrideVariableGroup.CustomVariableGroup.IdForEf == appWithGroup.CustomVariableGroup.IdForEf);
        }

        public IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            return this.Database.InstallationSummaries
                .Include(x => x.ApplicationServer)
                .Include(x => x.ApplicationWithOverrideVariableGroup)
                .OrderByDescending(x => x.InstallationStart)
                .Take(numberToRetrieve);
        }

        public void Save(InstallationSummary installationSummary)
        {
            // ToDo: Save child/list properties
            this.SaveChanges<InstallationSummary>(installationSummary);
        }
    }
}
