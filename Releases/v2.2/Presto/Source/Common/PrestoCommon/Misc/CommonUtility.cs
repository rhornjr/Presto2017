using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Misc
{
    public static class CommonUtility
    {
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
