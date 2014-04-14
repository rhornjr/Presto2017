using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryServerStart : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryServerStart()
        {
            Map = summaries => from summary in summaries
                               select new
                               {
                                   InstallationStartUtc = summary.InstallationStartUtc,
                                   ApplicationServerId = summary.ApplicationServerId
                               };
        }
    }
}
