using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://fs-12220", headers: "*", methods: "*")]
    public class InstallsController : ApiController
    {
        public IEnumerable<InstallationSummaryDto> Get()
        {
            IEnumerable<InstallationSummary> installationSummaries;
            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                installationSummaries = prestoWcf.Service.GetMostRecentByStartTime(50);
            }

            var installationSummaryDtos = new List<InstallationSummaryDto>();

            // Just use this until we can give the user the flexibility to choose a different time zone.
            var timeZoneHelper = new TimeZoneHelperThisComputer();

            foreach (InstallationSummary installationSummary in installationSummaries)
            {
                InstallationSummaryDto dto = new InstallationSummaryDto();
                dto.ApplicationName        = installationSummary.ApplicationWithOverrideVariableGroup.ToString();
                dto.Id                     = installationSummary.Id;
                dto.Result                 = installationSummary.InstallationResult.ToString();
                dto.ServerName             = installationSummary.ApplicationServer.Name;
                dto.TaskDetails            = installationSummary.TaskDetails;

                timeZoneHelper.SetStartAndEndTimes(installationSummary, dto);

                installationSummaryDtos.Add(dto);
            }

            return installationSummaryDtos.OrderByDescending(x => x.InstallationStart);
        }
    }
}