using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IApplicationService
    {
        [OperationContract]
        Application GetById(string id);

        [OperationContract]
        IEnumerable<Application> GetAllApplications(bool includeArchivedApps);

        [OperationContract]
        IEnumerable<ApplicationDtoSlim> GetAllApplicationsSlim();


        [OperationContract]
        Application GetByName(string name);

        [OperationContract]
        Application SaveApplication(Application application);
    }
}
