using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface ICustomVariableGroupService
    {
        [OperationContract]
        CustomVariableGroup GetById(string id);

        [OperationContract]
        IEnumerable<CustomVariableGroup> GetAllGroups();

        [OperationContract]
        CustomVariableGroup GetCustomVariableGroupByName(string name);

        [OperationContract]
        void DeleteGroup(CustomVariableGroup customVariableGroup);

        [OperationContract]
        CustomVariableGroup SaveGroup(CustomVariableGroup customVariableGroup);
    }
}
