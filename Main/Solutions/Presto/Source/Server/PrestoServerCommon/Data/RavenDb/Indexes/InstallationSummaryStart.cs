using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class InstallationSummaryStart : AbstractIndexCreationTask<InstallationSummary>
    {
        public InstallationSummaryStart()
        {
            Map = summaries => from summary in summaries
                               select new {
                                   ApplicationWithOverrideVariableGroup_CustomVariableGroupId = summary.ApplicationWithOverrideVariableGroup.CustomVariableGroupId,
                                   ApplicationWithOverrideVariableGroup_ApplicationId = summary.ApplicationWithOverrideVariableGroup.ApplicationId,
                                   ApplicationServerId = summary.ApplicationServerId,
                                   InstallationStart = summary.InstallationStart
                               };
        }
    }
}
