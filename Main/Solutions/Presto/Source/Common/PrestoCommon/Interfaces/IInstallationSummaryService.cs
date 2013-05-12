using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;

namespace PrestoCommon.Interfaces
{
    [ServiceContract]
    public interface IInstallationSummaryService
    {
        [OperationContract]
        IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve);
    }
}
