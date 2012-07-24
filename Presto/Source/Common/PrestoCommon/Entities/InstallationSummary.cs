using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PrestoCommon.Enums;

namespace PrestoCommon.Entities
{
    public class InstallationSummary : EntityBase
    {
        public string ApplicationServerId { get; set; }

        [JsonIgnore]
        public ApplicationServer ApplicationServer { get; set; }

        public ApplicationWithOverrideVariableGroup ApplicationWithOverrideVariableGroup { get; set; }

        public DateTime InstallationStart { get; set; }

        public DateTime InstallationEnd { get; set; }

        public InstallationResult InstallationResult { get; set; }

        public List<TaskDetail> TaskDetails { get; set; }

        public InstallationSummary(ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup, ApplicationServer applicationServer, DateTime startTime)
        {
            this.ApplicationWithOverrideVariableGroup = applicationWithOverrideVariableGroup;
            this.ApplicationServer                    = applicationServer;
            this.InstallationStart                    = startTime;
        }
    }
}
