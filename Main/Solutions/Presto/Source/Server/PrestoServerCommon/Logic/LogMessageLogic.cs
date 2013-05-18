using System;
using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Xanico.Core.Security;

namespace PrestoServer.Logic
{
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
            LogMessage logMessage = new LogMessage(message, DateTime.Now, IdentityHelper.UserName);

            DataAccessFactory.GetDataInterface<ILogMessageData>().Save(logMessage);
        }
    }
}
