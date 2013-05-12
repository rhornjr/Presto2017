using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Enums;
using Raven.Imports.Newtonsoft.Json;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class InstallationSummary : EntityBase
    {
        [DataMember]
        public string ApplicationServerId { get; set; }

        [JsonIgnore]  // We do not want RavenDB to serialize this...
        [DataMember]  // ... but we still want it to go over WCF
        public ApplicationServer ApplicationServer { get; set; }

        [DataMember]
        public ApplicationWithOverrideVariableGroup ApplicationWithOverrideVariableGroup { get; set; }

        [DataMember]
        public DateTime InstallationStart { get; set; }

        [DataMember]
        public DateTime InstallationEnd { get; set; }

        [DataMember]
        public DateTime InstallationStartUtc { get; set; }

        [DataMember]
        public DateTime InstallationEndUtc { get; set; }

        [DataMember]
        public InstallationResult InstallationResult { get; set; }

        [DataMember]
        public List<TaskDetail> TaskDetails { get; set; }

        public InstallationSummary(ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup, ApplicationServer applicationServer, DateTime startTime)
        {
            this.ApplicationWithOverrideVariableGroup = applicationWithOverrideVariableGroup;
            this.ApplicationServer                    = applicationServer;
            this.InstallationStart                    = startTime;
            this.InstallationStartUtc                 = TimeZoneInfo.ConvertTimeToUtc(startTime);
        }

        public void SetResults(InstallationResultContainer resultContainer, DateTime endTime)
        {
            if (resultContainer == null) { throw new ArgumentNullException("resultContainer"); }

            this.InstallationResult = resultContainer.InstallationResult;
            this.TaskDetails        = resultContainer.TaskDetails;
            this.InstallationEnd    = endTime;
            this.InstallationEndUtc = TimeZoneInfo.ConvertTimeToUtc(endTime);
        }
    }
}