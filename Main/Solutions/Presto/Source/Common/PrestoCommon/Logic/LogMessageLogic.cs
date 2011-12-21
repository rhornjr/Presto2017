using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class LogMessageLogic : LogicBase
    {
        /// <summary>
        /// Gets the most recent by created time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public static IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve)
        {
            IEnumerable<LogMessage> logMessages = (from LogMessage logMessage in Database
                                                   orderby logMessage.MessageCreatedTime descending
                                                   select logMessage).Take(numberToRetrieve);

            Database.Ext().Refresh(logMessages, 10);

            return logMessages;
        }

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
