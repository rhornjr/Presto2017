using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class InstallsController : ApiController
    {
        [AcceptVerbs("POST")]
        public IEnumerable<InstallationSummaryDto> Get(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            var installationSummaries = GetInstallationSummaries(appAndServerAndOverrides);

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

        private IEnumerable<InstallationSummary> GetInstallationSummaries(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            IEnumerable<InstallationSummary> installationSummaries;
            const int numberToRetrieve = 50;

            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                if (appAndServerAndOverrides.Application != null & appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeServerAndApplication(
                        numberToRetrieve, appAndServerAndOverrides.Server.Id, appAndServerAndOverrides.Application.Id);
                }

                if (appAndServerAndOverrides.Application != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndApplication(
                        numberToRetrieve, appAndServerAndOverrides.Application.Id);
                }

                if (appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndServer(
                        numberToRetrieve, appAndServerAndOverrides.Server.Id);
                }

                // No filter; just get the most recent.
                return prestoWcf.Service.GetMostRecentByStartTime(numberToRetrieve);
            }
        }
    }
}