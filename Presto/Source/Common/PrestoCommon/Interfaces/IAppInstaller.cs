using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    // Installing an app used to just be a method within the ApplicationServer class. But, it should be mocked
    // so we can test it without actually installing anything. That's why there's an interface now.
    public interface IAppInstaller
    {
        void InstallApplication(ApplicationServer server, ApplicationWithOverrideVariableGroup appWithGroup);
    }
}
