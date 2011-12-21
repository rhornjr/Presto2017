using System;
using System.Security.Principal;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class LogMessageLogic : LogicBase
    {
        /// <summary>
        /// Saves the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SaveLogMessage(string message)
        {
            LogMessage logMessage = new LogMessage(message, DateTime.Now, WindowsIdentity.GetCurrent().Name);

            Database.Store(logMessage);
            Database.Commit();
        }
    }
}
