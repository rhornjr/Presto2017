using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IBaseService
    {
        [OperationContract]
        string Echo(string message);

        [OperationContract]
        void Delete(EntityBase objectToDelete);

        [OperationContract]
        IEnumerable<LogMessage> GetMostRecentLogMessagesByCreatedTime(int numberToRetrieve);

        [OperationContract]
        void SaveLogMessage(string message);

        [OperationContract]
        GlobalSetting GetGlobalSettingItem();

        [OperationContract]
        GlobalSetting SaveGlobalSetting(GlobalSetting globalSetting);

        [OperationContract]
        string GetSignalRAddress();
    }
}
