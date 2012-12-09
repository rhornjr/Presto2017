using PrestoCommon.Entities;

namespace PrestoCommon.EntityHelperClasses.TimeZoneHelpers
{
    public interface ITimeZoneHelper
    {
        string TimeDisplayName { get; }
        void SetStartAndEndTimes(InstallationSummary installationSummary, InstallationSummaryDto dto);
    }
}
