using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryAppStart : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryAppStart()
        {
            Map = summaries => from summary in summaries
                               select new
                               {
                                   ApplicationWithOverrideVariableGroup_ApplicationId = summary.ApplicationWithOverrideVariableGroup.ApplicationId,
                                   InstallationStartUtc = summary.InstallationStartUtc
                               };
        }
    }
}
