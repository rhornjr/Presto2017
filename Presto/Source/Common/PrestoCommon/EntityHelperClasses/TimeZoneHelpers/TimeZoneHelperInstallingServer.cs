using System;
using PrestoCommon.Entities;

namespace PrestoCommon.EntityHelperClasses.TimeZoneHelpers
{
    public class TimeZoneHelperInstallingServer : ITimeZoneHelper
    {
        private const string _timeDisplayName = "Local (installing server)";

        public string TimeDisplayName
        {
            get { return _timeDisplayName; }
        }

        public void SetStartAndEndTimes(InstallationSummary installationSummary, InstallationSummaryDto dto)
        {
            if (installationSummary == null) { throw new ArgumentNullException("installationSummary"); }
            if (dto == null) { throw new ArgumentNullException("dto"); }

            dto.InstallationStart = installationSummary.InstallationStart;
            dto.InstallationEnd   = installationSummary.InstallationEnd;
        }

        public override string ToString()
        {
            return TimeDisplayName;
        }
    }
}
