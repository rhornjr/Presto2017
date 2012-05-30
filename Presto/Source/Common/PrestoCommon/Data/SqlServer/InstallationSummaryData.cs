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
            throw new NotImplementedException();
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
            this.SaveChanges<InstallationSummary>(installationSummary);
        }
    }
}
