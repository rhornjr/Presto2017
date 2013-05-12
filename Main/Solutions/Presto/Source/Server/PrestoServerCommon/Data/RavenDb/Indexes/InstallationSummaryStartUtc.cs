using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryStartUtc : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryStartUtc()
        {
            Map = summaries => from summary in summaries
                               select new { InstallationStartUtc = summary.InstallationStartUtc };
        }
    }
}
