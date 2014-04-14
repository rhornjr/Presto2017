using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryAppServerStart2 : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryAppServerStart2()
        {
            Map = summaries => from summary in summaries
                               select new {
                                   ApplicationWithOverrideVariableGroup_ApplicationId = summary.ApplicationWithOverrideVariableGroup.ApplicationId,
                                   ApplicationServerId = summary.ApplicationServerId
                               };
        }
    }
}
