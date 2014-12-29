using PrestoCommon.Entities;

namespace PrestoWeb.EntityContainers
{
    public class AppServerAndAppWithGroup
    {
        public ApplicationServer Server { get; set; }

        public ApplicationWithOverrideVariableGroup AppWithGroup { get; set; }
    }
}