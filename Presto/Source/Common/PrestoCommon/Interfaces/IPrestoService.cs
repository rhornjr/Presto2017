using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IPrestoService
    {
        [OperationContract]
        string Echo(string message);

        [OperationContract]
        IEnumerable<Application> GetAllApplications();
    }
}
