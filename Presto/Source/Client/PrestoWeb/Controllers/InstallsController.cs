using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using PrestoCommon.DataTransferObjects;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using Xanico.Core;

namespace PrestoWeb.Controllers
{
    // The origin is the web server.
    [EnableCors(origins: "http://apps.firstsolar.com", headers: "*", methods: "*")]
    public class InstallsController : ApiController
    {
        int _maxInstallsToRetrieve = Convert.ToInt32(ConfigurationManager.AppSettings["MaxInstallsToRetrieve"]);

        [AcceptVerbs("POST")]
        public IEnumerable<InstallationSummaryDto> Get(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            try
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
                    dto.Environment            = installationSummary.ApplicationServer.InstallationEnvironment.ToString();
                    dto.TaskDetails            = installationSummary.TaskDetails;

                    timeZoneHelper.SetStartAndEndTimes(installationSummary, dto);

                    installationSummaryDtos.Add(dto);
                }

                return installationSummaryDtos.OrderByDescending(x => x.InstallationStart);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw Helper.CreateHttpResponseException(ex, "Error Getting Installations");
            }
        }

        private IEnumerable<InstallationSummary> GetInstallationSummaries(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                if (appAndServerAndOverrides.Application != null & appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeServerAndApplication(
                        _maxInstallsToRetrieve, appAndServerAndOverrides.Server.Id, appAndServerAndOverrides.Application.Id,
                        appAndServerAndOverrides.EndDate);
                }

                if (appAndServerAndOverrides.Application != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndApplication(
                        _maxInstallsToRetrieve, appAndServerAndOverrides.Application.Id, appAndServerAndOverrides.EndDate);
                }

                if (appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndServer(
                        _maxInstallsToRetrieve, appAndServerAndOverrides.Server.Id, appAndServerAndOverrides.EndDate);
                }

                // No filter; just get the most recent.
                return prestoWcf.Service.GetMostRecentByStartTime(_maxInstallsToRetrieve, appAndServerAndOverrides.EndDate);
            }
        }
    }
}