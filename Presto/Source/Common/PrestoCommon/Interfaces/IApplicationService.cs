using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IApplicationService
    {
        [OperationContract]
        Application GetById(string id);

        [OperationContract]
        IEnumerable<Application> GetAllApplications();

        [OperationContract]
        Application GetByName(string name);

        [OperationContract]
        Application SaveApplication(Application application);
    }
}
