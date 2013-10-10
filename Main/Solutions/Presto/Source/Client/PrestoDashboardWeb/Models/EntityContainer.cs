using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;

namespace PrestoDashboardWeb.Models
{
    public class EntityContainer
    {
        public IEnumerable<Application> Applications { get; set; }
        public IEnumerable<ApplicationServer> Servers { get; set; }
        public IEnumerable<CustomVariableGroup> VariableGroups { get; set; }
        public IEnumerable<InstallationSummaryDto> InstallationSummaries { get; set; }
    }
}