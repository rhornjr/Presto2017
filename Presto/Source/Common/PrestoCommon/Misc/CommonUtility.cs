using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Unity;
using PrestoCommon.Entities;
using Xanico.Core;
using Xanico.Core.Email;

namespace PrestoCommon.Misc
{
    public static class CommonUtility
    {
        private static UnityContainer _container = new UnityContainer();

        public static UnityContainer Container
        {
            get { return _container; }
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

        /// <summary>
        /// Logs an exception and sends and email. The email will only be sent if the config file has email settings.
        /// </summary>
        /// <param name="ex"></param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static void ProcessException(Exception ex, string source = "")
        {
            if (ex == null) { return; }

            Logger.LogException(ex, source);

            EmailSettings emailSettings = new EmailSettings();

            emailSettings.EmailHost = ConfigurationManager.AppSettings["emailHost"];
            emailSettings.EmailFrom = ConfigurationManager.AppSettings["emailFrom"];
            emailSettings.EmailTo   = ConfigurationManager.AppSettings["emailTo"];

            if (emailSettings.EmailHost == null || emailSettings.EmailFrom == null || emailSettings.EmailTo == null)
            {
                Logger.LogWarning(String.Format(CultureInfo.CurrentCulture,
                    "An attempt was made to send an email for an exception, but email settings were not found in the app.config file. Stack trace: {0}",
                    Environment.StackTrace));
                return;
            }

            emailSettings.Subject = "Presto Exception - " + Environment.MachineName;
            emailSettings.Message = ex.ToString();

            EmailUtility.SendEmail(emailSettings);
        }
    }
}
