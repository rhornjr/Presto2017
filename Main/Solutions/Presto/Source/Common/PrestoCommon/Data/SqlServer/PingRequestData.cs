using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class PingRequestData : DataAccessLayerBase, IPingRequestData
    {
        public void Save(PingRequest pingRequest)
        {
            this.SaveChanges<PingRequest>(pingRequest);
        }

        public PingRequest GetMostRecent()
        {
            return this.Database.PingRequests.OrderByDescending(x => x.IdForEf).FirstOrDefault();
        }
    }
}
