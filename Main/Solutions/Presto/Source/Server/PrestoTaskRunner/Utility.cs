using System;
using System.Configuration;
using System.Globalization;
using PrestoCommon.Misc;
using Xanico.Core;
using System.Threading;

namespace PrestoTaskRunner
{
    /// <summary>
    /// Helper class for the PTR assembly
    /// </summary>
    public static class Utility
    {
        private static DateTime _lastEmailSent;

        /// <summary>
        /// Sets the Logger.Source property to the processName value from the app.config.
        /// </summary>
        public static void SetLoggerSource()
        {
            Logger.Source = ConfigurationManager.AppSettings["processName"];
        }

        internal static void ProcessException(Exception ex, string source = "")
        {
            // Don't email timeout exceptions. They occur too frequently (in Malaysia) to send emails.
            // ThreadAbortException happens when the PTR gets updated when updating the manifest file.
            if (ex is TimeoutException || ex is ThreadAbortException)
            {
                CommonUtility.ProcessException(ex, source, false);  // log only
                return;
            }

            int maxExceptionEmailFrequencyInSeconds =
                Convert.ToInt32(ConfigurationManager.AppSettings["maxExceptionEmailFrequencyInSeconds"], CultureInfo.InvariantCulture);

            bool shouldSendEmail = true;

            if (DateTime.Now.Subtract(_lastEmailSent).TotalSeconds < maxExceptionEmailFrequencyInSeconds) { shouldSendEmail = false; }

            if (shouldSendEmail) { _lastEmailSent = DateTime.Now; }

            // We always want to process the exception so it gets logged. We don't always want to
            // send an email so poor Bob doesn't get flooded with 800,000 of them when the servers
            // in Malaysia start timing out.
            CommonUtility.ProcessException(ex, source, shouldSendEmail);
        }
    }
}
