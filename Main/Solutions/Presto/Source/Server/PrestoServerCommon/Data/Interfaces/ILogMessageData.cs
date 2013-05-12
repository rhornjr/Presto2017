using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface ILogMessageData
    {
        /// <summary>
        /// Gets the most recent by created time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve);

        /// <summary>
        /// Saves the specified log message.
        /// </summary>
        /// <param name="logMessage">The log message.</param>
        void Save(LogMessage logMessage);
    }
}
