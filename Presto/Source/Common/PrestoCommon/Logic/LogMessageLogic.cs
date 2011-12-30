using System;
using System.Collections.Generic;
using System.Security.Principal;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogMessageLogic
    {
        /// <summary>
        /// Gets the most recent by created time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public static IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<ILogMessageData>().GetMostRecentByCreatedTime(numberToRetrieve);
        }

        /// <summary>
        /// Saves the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void SaveLogMessage(string message)
        {
            LogMessage logMessage = new LogMessage(message, DateTime.Now, WindowsIdentity.GetCurrent().Name);

            LogicBase.Save(logMessage);
        }
    }
}
