using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Unity;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Xanico.Core;
using Xanico.Core.Email;
using Xanico.Core.Security;

namespace PrestoCommon.Misc
{
    public static class CommonUtility
    {
        private static UnityContainer _container = new UnityContainer();
        private static string _signalRAddress;

        public static UnityContainer Container
        {
            get { return _container; }
        }

        public static string SignalRAddress
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_signalRAddress))
                {
                    using (var prestoWcf = new PrestoWcf<IBaseService>())
                    {
                        _signalRAddress = prestoWcf.Service.GetSignalRAddress();
                    }
                }
                return _signalRAddress;
            }
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
                return appWithGroupList.FirstOrDefault(groupFromList =>
                    groupFromList.Application.Id == appWithGroupToFind.Application.Id &&
                    groupFromList.CustomVariableGroup == null);
            }

            return appWithGroupList.FirstOrDefault(groupFromList =>
                groupFromList.Application.Id == appWithGroupToFind.Application.Id &&                
                groupFromList.CustomVariableGroup != null &&
                groupFromList.CustomVariableGroup.Id == appWithGroupToFind.CustomVariableGroup.Id);
        }

        public static ServerForceInstallation GetAppWithGroup(
            IEnumerable<ServerForceInstallation> forceInstallationsToDo, ApplicationWithOverrideVariableGroup appWithGroupToFind)
        {
            if (forceInstallationsToDo == null) { throw new ArgumentNullException("forceInstallationsToDo"); }
            if (appWithGroupToFind == null) { throw new ArgumentNullException("appWithGroupToFind"); }

            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            try
            {
                if (appWithGroupToFind.CustomVariableGroup == null)
                {
                    return forceInstallationsToDo.FirstOrDefault(forceInstallationToDo =>
                        forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                        forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup == null);
                }

                return forceInstallationsToDo.FirstOrDefault(forceInstallationToDo =>
                    forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup != null &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroup.Id == appWithGroupToFind.CustomVariableGroupId);
            }
            catch
            {
                try
                {
                    Logger.LogObjectDump(forceInstallationsToDo, "forceInstallationsToDo");
                    Logger.LogObjectDump(appWithGroupToFind, "appWithGroupToFind");
                }
                catch
                {
                    // Eat it. We don't want logging object dumps to stop us from throwing the initial exception.
                }
                throw;
            }
        }

        /// <summary>
        /// Logs an exception and sends and email. The email will only be sent if the config file has email settings.
        /// </summary>
        /// <param name="ex"></param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static void ProcessException(Exception ex, string source = "")
        {
            try
            {
                if (ex == null)
                {
                    Logger.LogWarning(String.Format(CultureInfo.CurrentCulture,
                        "An attempt was made to send an email for an exception, but the exception object was null. Stack trace: {0}",
                        Environment.StackTrace));
                    return;
                }

                Logger.LogException(ex, source);

                EmailSettings emailSettings = new EmailSettings();

                emailSettings.EmailHost = ConfigurationManager.AppSettings["emailHost"];
                emailSettings.EmailFrom = ConfigurationManager.AppSettings["emailFrom"];
                emailSettings.EmailTo = ConfigurationManager.AppSettings["emailTo"];

                if (emailSettings.EmailHost == null || emailSettings.EmailFrom == null || emailSettings.EmailTo == null)
                {
                    Logger.LogWarning(String.Format(CultureInfo.CurrentCulture,
                        "An attempt was made to send an email for an exception, but one or more of the email settings " +
                        "(emailHost, emailFrom, emailTo) were not found in the app.config file. Stack trace: {0}",
                        Environment.StackTrace));
                    return;
                }

                emailSettings.Subject = "Presto Exception - " + Environment.MachineName + " - " + IdentityHelper.UserName;
                emailSettings.Message = "Time: " + DateTime.Now + Environment.NewLine +
                                        "Server: " + Environment.MachineName + Environment.NewLine +
                                        "User: " + IdentityHelper.UserName + Environment.NewLine + Environment.NewLine +
                                        ex;

                EmailUtility.SendEmail(emailSettings);
            }
            catch (Exception exception)
            {
                try
                {
                    Logger.LogException(exception);
                }
                catch
                {
                    // Eat it. If we can't log while processing an exception, there is nothing else we can do.
                    // We don't want to throw an exception while processing an exception.
                }
            }
        }
    }
}
