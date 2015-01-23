using System;
using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Xanico.Core.Security;
using System.Diagnostics;
using System.Security.Principal;

namespace PrestoServer.Logic
{
    public static class LogMessageLogic
    {
        public static IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve)
        {
            return DataAccessFactory.GetDataInterface<ILogMessageData>().GetMostRecentByCreatedTime(numberToRetrieve);
        }

        public static void SaveLogMessage(string message)
        {
            // Getting the user name even works with the Presto web request because Windows authentication
            // and impersonation are on in IIS.
            LogMessage logMessage = new LogMessage(message, DateTime.Now, IdentityHelper.UserName);

            DataAccessFactory.GetDataInterface<ILogMessageData>().Save(logMessage);
        }
    }
}
