using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IInstallationEnvironmentService
    {
        [OperationContract]
        IEnumerable<InstallationEnvironment> GetAllInstallationEnvironments();

        [OperationContract]
        void Save(InstallationEnvironment environment);
    }
}
