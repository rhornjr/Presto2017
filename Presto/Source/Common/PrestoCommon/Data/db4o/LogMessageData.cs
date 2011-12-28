using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
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
            IEnumerable<LogMessage> logMessages = (from LogMessage logMessage in Database
                                                   orderby logMessage.MessageCreatedTime descending
                                                   select logMessage).Take(numberToRetrieve);

            Database.Ext().Refresh(logMessages, 10);

            return logMessages;
        }
    }
}
