using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoCommon.Data.RavenDb.Indexes
{
    public class InstallationSummaryStart : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryStart()
        {
            Map = summaries => from summary in summaries
                               select new { InstallationStart = summary.InstallationStart };
        }
    }
}
