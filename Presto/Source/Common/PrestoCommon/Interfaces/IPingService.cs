using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IPingService
    {
        [OperationContract]
        PingRequest GetMostRecentPingRequest();

        [OperationContract]
        PingRequest SavePingRequest(PingRequest pingRequest);

        [OperationContract]
        IEnumerable<PingResponse> GetAllForPingRequest(PingRequest pingRequest);
    }
}
