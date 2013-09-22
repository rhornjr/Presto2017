using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoDashboardWeb.Models
{
    public class EntityContainer
    {
        public IEnumerable<Application> Applications { get; set; }
        public IEnumerable<ApplicationServer> Servers { get; set; }
        public IEnumerable<CustomVariableGroup> VariableGroups { get; set; }
    }
}