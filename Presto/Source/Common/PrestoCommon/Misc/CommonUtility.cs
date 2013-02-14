using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Data.RavenDb;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;

namespace PrestoCommon.Misc
{
    public static class CommonUtility
    {
        private static UnityContainer _container = new UnityContainer();

        public static UnityContainer Container
        {
            get { return _container; }
        }

        public static void RegisterRavenDataClasses()
        {
            CommonUtility.Container.RegisterType<IApplicationData,             ApplicationData>();
            CommonUtility.Container.RegisterType<IApplicationServerData,       ApplicationServerData>();
            CommonUtility.Container.RegisterType<ICustomVariableGroupData,     CustomVariableGroupData>();
            CommonUtility.Container.RegisterType<IGenericData,                 GenericData>();
            CommonUtility.Container.RegisterType<IGlobalSettingData,           GlobalSettingData>();
            CommonUtility.Container.RegisterType<IInstallationSummaryData,     InstallationSummaryData>();
            CommonUtility.Container.RegisterType<ILogMessageData,              LogMessageData>();
            CommonUtility.Container.RegisterType<IPingRequestData,             PingRequestData>();
            CommonUtility.Container.RegisterType<IPingResponseData,            PingResponseData>();
            CommonUtility.Container.RegisterType<IInstallationEnvironmentData, InstallationEnvironmentData>();
        }

        public static void RegisterRealClasses()
        {
            // This is so we can make the actual app installations. When running test code, we don't
            // want an app to actually be installed.
            CommonUtility.Container.RegisterType<IAppInstaller, AppInstaller>();
        }

        public static ApplicationWithOverrideVariableGroup GetAppWithGroup(
            IEnumerable<ApplicationWithOverrideVariableGroup> appWithGroupList, ApplicationWithOverrideVariableGroup appWithGroupToFind)
        {
            if (appWithGroupList == null) { throw new ArgumentNullException("appWithGroupList"); }
            if (appWithGroupToFind == null) { throw new ArgumentNullException("appWithGroupToFind"); }

            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            if (appWithGroupToFind.CustomVariableGroup == null)
            {
                return appWithGroupList.Where(groupFromList =>
                    groupFromList.Application.Id == appWithGroupToFind.Application.Id &&
                    groupFromList.CustomVariableGroup == null)
                    .FirstOrDefault();
            }

            return appWithGroupList.Where(groupFromList =>
                groupFromList.Application.Id == appWithGroupToFind.Application.Id &&                
                groupFromList.CustomVariableGroup != null &&
                groupFromList.CustomVariableGroup.Id == appWithGroupToFind.CustomVariableGroup.Id)
                .FirstOrDefault();
        }

        public static ServerForceInstallation GetAppWithGroup(
            IEnumerable<ServerForceInstallation> forceInstallationsToDo, ApplicationWithOverrideVariableGroup appWithGroupToFind)
        {
            if (forceInstallationsToDo == null) { throw new ArgumentNullException("forceInstallationsToDo"); }
            if (appWithGroupToFind == null) { throw new ArgumentNullException("appWithGroupToFind"); }

            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            if (appWithGroupToFind.CustomVariableGroup == null)
            {
                return forceInstallationsToDo.Where(forceInstallationToDo =>
                    forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup == null)
                    .FirstOrDefault();
            }

            return forceInstallationsToDo.Where(forceInstallationToDo =>
                forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup != null && 
                forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup.Id == appWithGroupToFind.CustomVariableGroupId)
                .FirstOrDefault();
        }
    }
}
