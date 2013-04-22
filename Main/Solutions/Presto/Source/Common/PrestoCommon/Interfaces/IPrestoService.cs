using System.ServiceModel;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IPrestoService
    {
        [OperationContract]
        string Echo(string message);
    }
}
