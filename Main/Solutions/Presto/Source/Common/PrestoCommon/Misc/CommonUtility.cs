using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Misc
{
    /// <summary>
    /// Common helper methods
    /// </summary>
    public static class CommonUtility
    {
        /// <summary>
        /// Gets the app with group.
        /// </summary>
        /// <param name="appWithGroupList">The app with group list.</param>
        /// <param name="appWithGroupToFind">The app with group to find.</param>
        /// <returns></returns>
        public static ApplicationWithOverrideVariableGroup GetAppWithGroup(
            IEnumerable<ApplicationWithOverrideVariableGroup> appWithGroupList, ApplicationWithOverrideVariableGroup appWithGroupToFind)
        {
            if (appWithGroupList == null) { throw new ArgumentNullException("appWithGroupList"); }
            if (appWithGroupToFind == null) { throw new ArgumentNullException("appWithGroupToFind"); }

            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            return appWithGroupList.Where(groupFromList =>
                groupFromList.ApplicationId == appWithGroupToFind.ApplicationId &&
                ((groupFromList.CustomVariableGroupId == null && appWithGroupToFind.CustomVariableGroupId == null) ||
                (groupFromList.CustomVariableGroupId == appWithGroupToFind.CustomVariableGroupId))).FirstOrDefault();
        }
    }
}
