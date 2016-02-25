using Microsoft.Practices.Unity;
using PrestoCommon.Interfaces;
using PrestoServer.Data.Interfaces;
using PrestoServer.Data.RavenDb;
using PrestoServer.Misc;

namespace PrestoServer
{
    public static class PrestoServerUtility
    {
        private static UnityContainer _container = new UnityContainer();

        public static UnityContainer Container
        {
            get { return _container; }
        }

        public static void RegisterRavenDataClasses()
        {
            Container.RegisterType<IApplicationData, ApplicationData>();
            Container.RegisterType<IApplicationServerData, ApplicationServerData>();
            Container.RegisterType<ICustomVariableGroupData, CustomVariableGroupData>();
            Container.RegisterType<IGenericData, GenericData>();
            Container.RegisterType<IGlobalSettingData, GlobalSettingData>();
            Container.RegisterType<IInstallationSummaryData, InstallationSummaryData>();
            Container.RegisterType<IInstallationsPendingData, InstallationsPendingData>();
            Container.RegisterType<ILogMessageData, LogMessageData>();
            Container.RegisterType<IPingRequestData, PingRequestData>();
            Container.RegisterType<IPingResponseData, PingResponseData>();
            Container.RegisterType<IInstallationEnvironmentData, InstallationEnvironmentData>();
            Container.RegisterType<ISecurityData, SecurityData>();
        }

        public static void RegisterRealClasses()
        {
            // This is so we can mock the actual app installations. When running test code, we don't
            // want an app to actually be installed.
            Container.RegisterType<IAppInstaller, AppInstaller>();
        }
    }
}
