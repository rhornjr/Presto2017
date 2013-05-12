using System.Linq;
using PrestoCommon.Entities;
using Raven.Client.Indexes;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class LogMessageCreatedTime : AbstractIndexCreationTask<LogMessage>
    {
        public LogMessageCreatedTime()
        {
            Map = messages => from message in messages
                              select new { MessageCreatedTime = message.MessageCreatedTime };
        }
    }
}
