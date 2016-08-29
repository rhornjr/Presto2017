using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
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
        int _defaultInstallsToRetrieve = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultInstallsToRetrieve"]);
        int _maxInstallsToRetrieve = Convert.ToInt32(ConfigurationManager.AppSettings["MaxInstallsToRetrieve"]);

        [AcceptVerbs("POST")]
        public IEnumerable<InstallationSummaryDto> Get(AppAndServerAndOverrides appAndServerAndOverrides)
        {
            try
            {
                if (appAndServerAndOverrides.MaxResults <= 0)
                {
                    appAndServerAndOverrides.MaxResults = _defaultInstallsToRetrieve;
                }

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
            if (appAndServerAndOverrides.MaxResults > _maxInstallsToRetrieve)
            {
                string message = string.Format(CultureInfo.CurrentCulture,
                    "Cannot retrieve {0} records. The maximum allowed is {1}.",
                    appAndServerAndOverrides.MaxResults,
                    _maxInstallsToRetrieve);

                throw new InvalidOperationException(message);
            }

            using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
            {
                if (appAndServerAndOverrides.Application != null & appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeServerAndApplication(
                        appAndServerAndOverrides.MaxResults, appAndServerAndOverrides.Server.Id, appAndServerAndOverrides.Application.Id,
                        appAndServerAndOverrides.EndDate);
                }

                if (appAndServerAndOverrides.Application != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndApplication(
                        appAndServerAndOverrides.MaxResults, appAndServerAndOverrides.Application.Id, appAndServerAndOverrides.EndDate);
                }

                if (appAndServerAndOverrides.Server != null)
                {
                    return prestoWcf.Service.GetMostRecentByStartTimeAndServer(
                        appAndServerAndOverrides.MaxResults, appAndServerAndOverrides.Server.Id, appAndServerAndOverrides.EndDate);
                }

                // No filter; just get the most recent.
                return prestoWcf.Service.GetMostRecentByStartTime(appAndServerAndOverrides.MaxResults, appAndServerAndOverrides.EndDate);
            }
        }

        [AcceptVerbs("POST")]
        [Route("api/installs/convertToCsv")]
        public string ConvertToCsv(IEnumerable<InstallationSummaryDto> summaries)
        {
            var builder = new StringBuilder();
            var delimiter = ",";

            foreach (var summary in summaries)
            {
                builder.AppendLine(
                    summary.Id + delimiter +
                    summary.ApplicationName + delimiter +
                    summary.ServerName + delimiter +
                    summary.Environment + delimiter +
                    summary.InstallationStart + delimiter +
                    summary.InstallationEnd + delimiter +
                    summary.Result);
            }

            return builder.ToString();
        }

        [AcceptVerbs("GET")]
        [Route("api/installs/getNumberOfInstallsToRetrieve")]
        public int GetNumberOfInstallsToRetrieve()
        {
            return this._defaultInstallsToRetrieve;
        }

        [AcceptVerbs("GET")]
        [Route("api/installs/getMaxNumberOfInstallsToRetrieve")]
        public int GetMaxNumberOfInstallsToRetrieve()
        {
            return this._maxInstallsToRetrieve;
        }
    }
}