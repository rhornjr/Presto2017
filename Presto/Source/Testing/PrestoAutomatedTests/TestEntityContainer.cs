using PrestoCommon.Entities;

namespace PrestoAutomatedTests
{
    internal class TestEntityContainer
    {
        internal ApplicationServer ApplicationServer { get; set; }

        internal ApplicationWithOverrideVariableGroup AppWithGroup { get; set; }

        internal InstallationSummary MostRecentInstallationSummary { get; set; }

        internal ForceInstallation ForceInstallation { get; set; }
    }
}
