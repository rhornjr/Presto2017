using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        private static DateTime _lastEmailSent;
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

            if (appWithGroupToFind.CustomVariableGroups == null || appWithGroupToFind.CustomVariableGroups.Count < 1)
            {
                return appWithGroupList.FirstOrDefault(groupFromList =>
                    groupFromList.Application.Id == appWithGroupToFind.Application.Id &&
                    (groupFromList.CustomVariableGroups == null || groupFromList.CustomVariableGroups.Count < 1));
            }

            return appWithGroupList.FirstOrDefault(groupFromList =>
                groupFromList.Application.Id == appWithGroupToFind.Application.Id &&                
                groupFromList.CustomVariableGroups != null &&
                groupFromList.CustomVariableGroups.Count == appWithGroupToFind.CustomVariableGroups.Count &&
                groupFromList.CustomVariableGroups.Select(x => x.Id).All(appWithGroupToFind.CustomVariableGroups.Select(x => x.Id).Contains));
        }

        public static ServerForceInstallation GetForceInstallationContainingAppWithGroup(
            IEnumerable<ServerForceInstallation> forceInstallationsToDo, ApplicationWithOverrideVariableGroup appWithGroupToFind)
        {
            if (forceInstallationsToDo == null) { throw new ArgumentNullException("forceInstallationsToDo"); }
            if (appWithGroupToFind == null) { throw new ArgumentNullException("appWithGroupToFind"); }

            if (forceInstallationsToDo.Count() < 1) { return null; }

            // To find the matching appWithGroup, the app IDs need to match AND one of these two things must be true:
            // 1. Both custom variable groups are null, or
            // 2. Both custom variable groups are the same.

            try
            {
                if (appWithGroupToFind.CustomVariableGroups == null || appWithGroupToFind.CustomVariableGroups.Count < 1)
                {
                    return forceInstallationsToDo.FirstOrDefault(forceInstallationToDo =>
                        forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                        (forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroups == null
                        || forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroups.Count < 1));
                }

                return forceInstallationsToDo.FirstOrDefault(forceInstallationToDo =>
                    forceInstallationToDo.ApplicationWithOverrideGroup.Application.Id == appWithGroupToFind.ApplicationId &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroups != null &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroups.Count == appWithGroupToFind.CustomVariableGroups.Count &&
                    forceInstallationToDo.ApplicationWithOverrideGroup.CustomVariableGroups.Select(x => x.Id).All(appWithGroupToFind.CustomVariableGroups.Select(x => x.Id).Contains));

                // Note: The LINQ extension All() (above): Determines whether all elements of a sequence satisfy a condition. And we're
                //       passing the Contains() method from the other list as a delegate.
                //       Since we're making sure the counts are the same, the order of those lists (which one calls All()) doesn't
                //       matter.)
                //       Also, the order of the elements within each list doesn't matter.
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

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification="Tiring overhead")]
        public static void ProcessExceptionWithLimits(Exception ex, string source = "")
        {
            // If a source wasn't passed in, but it's in the config file, use the value from the config file.
            string sourceFromConfig = ConfigurationManager.AppSettings["processName"];
            if (string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(sourceFromConfig))
            {
                source = sourceFromConfig;
            }

            // Don't email timeout exceptions. They occur too frequently (in Malaysia) to send emails.
            // ThreadAbortException happens when the PTR gets updated when updating the manifest file.
            if (ex is TimeoutException || ex is ThreadAbortException)
            {
                ProcessException(ex, source, false);  // log only
                return;
            }

            int maxExceptionEmailFrequencyInSeconds = 360; // default if no app setting exists
            string maxFrequencyAsString = ConfigurationManager.AppSettings["maxExceptionEmailFrequencyInSeconds"];
            if (!string.IsNullOrWhiteSpace(maxFrequencyAsString))
            {
                maxExceptionEmailFrequencyInSeconds =
                    Convert.ToInt32(maxFrequencyAsString, CultureInfo.InvariantCulture);
            }

            bool shouldSendEmail = true;
            if (DateTime.Now.Subtract(_lastEmailSent).TotalSeconds < maxExceptionEmailFrequencyInSeconds)
            {
                shouldSendEmail = false;
            }

            if (shouldSendEmail) { _lastEmailSent = DateTime.Now; }

            // We always want to process the exception so it gets logged. We don't always want to
            // send an email so poor Bob doesn't get flooded with 800,000 of them when the servers
            // in Malaysia start timing out.
            ProcessException(ex, source, shouldSendEmail);
        }

        /// <summary>
        /// Logs an exception and sends and email. The email will only be sent if the config file has email settings.
        /// </summary>
        /// <param name="ex"></param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static void ProcessException(Exception ex, string source = "", bool sendEmail = true)
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

                string subject = "Presto Exception - " + Environment.MachineName + " - " + IdentityHelper.UserName;
                string message = "Time: " + DateTime.Now + Environment.NewLine +
                                 "Server: " + Environment.MachineName + Environment.NewLine +
                                 "User: " + IdentityHelper.UserName + Environment.NewLine + Environment.NewLine +
                                 ex;

                if (sendEmail) { SendEmail(subject, message); }
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

        /// <summary>
        /// Sends an email with the specified subject and message. The host config file is used for the email
        /// host, from, and to properties.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static void SendEmail(string subject, string message, string emailToConfigKey = "emailTo")
        {
            EmailSettings emailSettings = new EmailSettings();

            emailSettings.EmailHost = ConfigurationManager.AppSettings["emailHost"];
            emailSettings.EmailFrom = ConfigurationManager.AppSettings["emailFrom"];
            emailSettings.EmailTo   = ConfigurationManager.AppSettings[emailToConfigKey];

            if (emailSettings.EmailHost == null || emailSettings.EmailFrom == null || emailSettings.EmailTo == null)
            {
                Logger.LogWarning(String.Format(CultureInfo.CurrentCulture,
                    "An attempt was made to send an email, but one or more of the email settings " +
                    "(emailHost, emailFrom, emailTo) were not found in the app.config file. Stack trace: {0}",
                    Environment.StackTrace));
                return;
            }

            emailSettings.Subject = subject;
            emailSettings.Message = message;

            EmailUtility.SendEmail(emailSettings);
        }
    }
}
