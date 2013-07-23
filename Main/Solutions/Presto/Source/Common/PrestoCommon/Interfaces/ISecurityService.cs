using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface ISecurityService
    {
        [OperationContract]
        IEnumerable<AdGroupWithRoles> GetAllAdGroupWithRoles();

        [OperationContract]
        AdGroupWithRoles SaveAdGroupWithRoles(AdGroupWithRoles adGroupWithRoles);

        [OperationContract]
        ActiveDirectoryInfo GetActiveDirectoryInfo();

        [OperationContract]
        ActiveDirectoryInfo SaveActiveDirectoryInfo(ActiveDirectoryInfo adInfo);
    }
}
