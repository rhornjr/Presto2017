using System;
using System.Diagnostics;
using System.Reflection;

namespace PrestoCommon.Misc
{
    /// <summary>
    /// Quick and dirty logger to get us going
    /// </summary>
    public static class LogUtility
    {
        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogInformation(string message)
        {
            Log(message, EventLogEntryType.Information);
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogWarning(string message)
        {
            Log(message, EventLogEntryType.Warning);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void LogException(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            if (Environment.UserInteractive)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            EventLog.WriteEntry(GetSource(), ex.ToString(), EventLogEntryType.Error);
        }

        private static void Log(string message, EventLogEntryType entryType)
        {
            EventLog.WriteEntry(GetSource(), message, entryType);

            // If we're running a console app, also write the message to the console window.
            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
        }

        private static string GetSource()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }
    }
}
