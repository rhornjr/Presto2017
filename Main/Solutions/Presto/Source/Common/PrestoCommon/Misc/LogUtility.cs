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
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public static void LogException(Exception ex)
        {
            if (ex == null) { throw new ArgumentNullException("ex"); }

            EventLog.WriteEntry(GetSource(), ex.ToString(), EventLogEntryType.Error);
        }

        private static string GetSource()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }
    }
}
