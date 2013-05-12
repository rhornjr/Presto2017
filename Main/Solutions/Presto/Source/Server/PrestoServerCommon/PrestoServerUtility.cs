using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Practices.Unity;
using PrestoCommon.Interfaces;
using PrestoServer.Data.Interfaces;
using PrestoServer.Data.RavenDb;
using PrestoServer.Misc;

namespace PrestoServer
{
    public class PrestoServerUtility
    {
        private static UnityContainer _container = new UnityContainer();

        public static UnityContainer Container
        {
            get { return _container; }
        }

        public static void RegisterRavenDataClasses()
        {
            PrestoServerUtility.Container.RegisterType<IApplicationData, ApplicationData>();
            PrestoServerUtility.Container.RegisterType<IApplicationServerData, ApplicationServerData>();
            PrestoServerUtility.Container.RegisterType<ICustomVariableGroupData, CustomVariableGroupData>();
            PrestoServerUtility.Container.RegisterType<IGenericData, GenericData>();
            PrestoServerUtility.Container.RegisterType<IGlobalSettingData, GlobalSettingData>();
            PrestoServerUtility.Container.RegisterType<IInstallationSummaryData, InstallationSummaryData>();
            PrestoServerUtility.Container.RegisterType<ILogMessageData, LogMessageData>();
            PrestoServerUtility.Container.RegisterType<IPingRequestData, PingRequestData>();
            PrestoServerUtility.Container.RegisterType<IPingResponseData, PingResponseData>();
            PrestoServerUtility.Container.RegisterType<IInstallationEnvironmentData, InstallationEnvironmentData>();
        }

        public static void RegisterRealClasses()
        {
            // This is so we can mock the actual app installations. When running test code, we don't
            // want an app to actually be installed.
            PrestoServerUtility.Container.RegisterType<IAppInstaller, AppInstaller>();
        }

        /// <summary>
        /// Gets the file version. This method will not throw an exception and it will not return null.
        /// </summary>
        /// <returns></returns>
        public static string GetFileVersion(Assembly assembly)
        {
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                return fileVersionInfo.ProductVersion;
            }
            catch (Exception ex)
            {
                ServerLogUtility.LogException(ex);
                return string.Empty;
            }
        }
    }
}
