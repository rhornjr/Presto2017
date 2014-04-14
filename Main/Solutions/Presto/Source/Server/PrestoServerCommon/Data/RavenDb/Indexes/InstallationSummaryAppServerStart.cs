using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryAppServerStart : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryAppServerStart()
        {
            Map = summaries => from summary in summaries
                               select new {
                                   ApplicationWithOverrideVariableGroup_CustomVariableGroupId = summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId,
                                   ApplicationWithOverrideVariableGroup_ApplicationId = summary.ApplicationWithOverrideVariableGroup.ApplicationId,
                                   ApplicationWithOverrideVariableGroup = summary.ApplicationWithOverrideVariableGroup,
                                   ApplicationServerId = summary.ApplicationServerId,
                                   InstallationStart = summary.InstallationStart
                               };
        }
    }
}
