using System;
using System.Collections.Generic;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

namespace PrestoCommon.Data.RavenDb
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
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Advanced.LuceneQuery<LogMessage>()
                    .OrderBy("MessageCreatedTime")  // ToDo: This needs to be DESC
                    .Take(numberToRetrieve);
            }
        }

        /// <summary>
        /// Saves the log message.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        public void SaveLogMessage(LogMessage logMessage)
        {
            throw new NotImplementedException();
        }
    }
}
