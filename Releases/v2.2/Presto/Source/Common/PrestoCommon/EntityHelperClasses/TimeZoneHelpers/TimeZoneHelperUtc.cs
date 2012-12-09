using System;
using PrestoCommon.Entities;

namespace PrestoCommon.EntityHelperClasses.TimeZoneHelpers
{
    public class TimeZoneHelperUtc : ITimeZoneHelper
    {
        private const string _timeDisplayName = "UTC";

        public string TimeDisplayName
        {
            get { return _timeDisplayName; }
        }

        public void SetStartAndEndTimes(InstallationSummary installationSummary, InstallationSummaryDto dto)
        {
            if (installationSummary == null) { throw new ArgumentNullException("installationSummary"); }
            if (dto == null) { throw new ArgumentNullException("dto"); }

            dto.InstallationStart = installationSummary.InstallationStartUtc;
            dto.InstallationEnd   = installationSummary.InstallationEndUtc;
        }

        public override string ToString()
        {
            return TimeDisplayName;
        }
    }
}
