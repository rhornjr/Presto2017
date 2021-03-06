﻿using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class LogMessageData : DataAccessLayerBase, ILogMessageData
    {
        /// <summary>
        /// Gets the most recent by created time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve)
        {
            return ExecuteQuery<IEnumerable<LogMessage>>(() =>
                QueryAndSetEtags(session => session.Query<LogMessage>()
                .Customize(x => x.WaitForNonStaleResults())
                .OrderByDescending(logMessage => logMessage.MessageCreatedTime)
                .Take(numberToRetrieve)).AsEnumerable().Cast<LogMessage>()
                );
        }

        /// <summary>
        /// Saves the specified log message.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        public void Save(LogMessage logMessage)
        {
            new GenericData().Save(logMessage);
        }
    }
}
