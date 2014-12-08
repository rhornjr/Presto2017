using System;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.RavenDb;

namespace ConsoleTestRunner.RavenTestDataClasses
{
    internal class SandboxData : DataAccessLayerBase
    {
        internal static void VerifyGroupNotUsedByInstallationSummary(CustomVariableGroup cvg)
        {
            // Contains() not supported in RavenDB:
            //EntityBase installationSummary = QuerySingleResultAndSetEtag(session => session.Query<InstallationSummary>()
            //        .FirstOrDefault(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.Contains(customVariableGroup.Id)));

            // So here is my attempt to work around that:
            EntityBase installationSummary = QuerySingleResultAndSetEtag(session => session.Query<InstallationSummary>()
                    .FirstOrDefault(x => x.ApplicationWithOverrideVariableGroup.CustomVariableGroupIds.Any(y => y == cvg.Id)));

            Console.WriteLine(installationSummary == null);
        }
    }
}
