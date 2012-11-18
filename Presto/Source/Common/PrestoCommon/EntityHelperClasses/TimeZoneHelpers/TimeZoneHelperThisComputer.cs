using System;
using PrestoCommon.Entities;

namespace PrestoCommon.EntityHelperClasses.TimeZoneHelpers
{
    public class TimeZoneHelperThisComputer : ITimeZoneHelper
    {
        private string _timeDisplayName = TimeZone.CurrentTimeZone.StandardName;

        public string TimeDisplayName
        {
            get { return _timeDisplayName; }
        }

        public void SetStartAndEndTimes(InstallationSummary installationSummary, InstallationSummaryDto dto)
        {
            if (installationSummary == null) { throw new ArgumentNullException("installationSummary"); }
            if (dto == null) { throw new ArgumentNullException("dto"); }

            dto.InstallationStart = TimeZone.CurrentTimeZone.ToLocalTime(installationSummary.InstallationStartUtc);
            dto.InstallationEnd   = TimeZone.CurrentTimeZone.ToLocalTime(installationSummary.InstallationEndUtc);
        }

        public override string ToString()
        {
            return TimeDisplayName;
        }
    }
}
