using PrestoCommon.Entities;
using PrestoCommon.Interfaces;

namespace PrestoAutomatedTests.Mocks
{
    public class MockAppInstaller : IAppInstaller
    {
        public bool Invoked { get; set; }

        public void InstallApplication(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            this.Invoked = true;
        }
    }
}
