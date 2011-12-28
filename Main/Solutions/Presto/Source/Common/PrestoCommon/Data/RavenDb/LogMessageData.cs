using System.Collections.Generic;
using System.Linq;
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
                return session.Query<LogMessage>()
                    .OrderByDescending(logMessage => logMessage.MessageCreatedTime)
                    .Take(numberToRetrieve);

                //return session.Advanced.LuceneQuery<LogMessage>()
                //    .OrderBy("-MessageCreatedTime")
                //    .Take(numberToRetrieve);
            }
        }
    }
}
