using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class PingResponseData : DataAccessLayerBase, IPingResponseData
    {
        public void Save(PingResponse pingResponse)
        {
            throw new NotImplementedException();
        }

        public PingResponse GetByPingRequestAndServer(PingRequest pingRequest, ApplicationServer appServer)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest)
        {
            return this.Database.PingResponses.Where(x => x.PingRequest.IdForEf == pingRequest.IdForEf).ToList();
        }
    }
}
