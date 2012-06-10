using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class LogMessageData : DataAccessLayerBase, ILogMessageData
    {
        public IEnumerable<LogMessage> GetMostRecentByCreatedTime(int numberToRetrieve)
        {
            return this.Database.LogMessages
                .OrderByDescending(x => x.MessageCreatedTime)
                .Take(numberToRetrieve);
        }

        public void Save(LogMessage logMessage)
        {
            this.SaveChanges<LogMessage>(logMessage);
        }
    }
}
