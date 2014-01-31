using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoDashboardWeb.Models;

namespace PrestoDashboardWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult IndexNew()
        {
            var container = new EntityContainer();

            //var app1 = new Application() { Name = "app1", Archived = false, Version = "1.0", Etag = Guid.NewGuid() };
            //var app2 = new Application() { Name = "app2", Archived = false, Version = "2.0", Etag = Guid.NewGuid() };
            //var app3 = new Application() { Name = "app3", Archived = false, Version = "3.0", Etag = Guid.NewGuid() };
            //var app4 = new Application() { Name = "bapp4", Archived = false, Version = "4.0", Etag = Guid.NewGuid() };
            //apps.Add(app1);
            //apps.Add(app2);
            //apps.Add(app3);
            //apps.Add(app4);

            // ToDo: These should all be done with *one call* to the service.

            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                container.Applications = prestoWcf.Service.GetAllApplications(true).OrderBy(x => x.Name);
            }

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                container.Servers = prestoWcf.Service.GetAllServersSlim().OrderBy(x => x.Name);
            }

            using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
            {
                container.VariableGroups = prestoWcf.Service.GetAllGroups().OrderBy(x => x.Name);
            }

            container.InstallationSummaries = GetInstallations();

            return View(container);
        }

        public ActionResult Index()
        {
            var container = new EntityContainer();

            //var app1 = new Application() { Name = "app1", Archived = false, Version = "1.0", Etag = Guid.NewGuid() };
            //var app2 = new Application() { Name = "app2", Archived = false, Version = "2.0", Etag = Guid.NewGuid() };
            //var app3 = new Application() { Name = "app3", Archived = false, Version = "3.0", Etag = Guid.NewGuid() };
            //var app4 = new Application() { Name = "bapp4", Archived = false, Version = "4.0", Etag = Guid.NewGuid() };
            //apps.Add(app1);
            //apps.Add(app2);
            //apps.Add(app3);
            //apps.Add(app4);

            // ToDo: These should all be done with *one call* to the service.

            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                container.Applications = prestoWcf.Service.GetAllApplications(true).OrderBy(x => x.Name);
            }

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                container.Servers = prestoWcf.Service.GetAllServersSlim().OrderBy(x => x.Name);
            }

            using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
            {
                container.VariableGroups = prestoWcf.Service.GetAllGroups().OrderBy(x => x.Name);
            }

            container.InstallationSummaries = GetInstallations();

            return View(container);
        }

        private static IEnumerable<InstallationSummaryDto> GetInstallations()
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

                timeZoneHelper.SetStartAndEndTimes(installationSummary, dto);

                installationSummaryDtos.Add(dto);
            }

            return installationSummaryDtos.OrderByDescending(x => x.InstallationStart);
        }

        [HttpPost]
        public ActionResult GetAppById(string appId)
        {
            Application app = null;

            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                app = prestoWcf.Service.GetById(appId);
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = app;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult GetServerById(string serverId)
        {
            ApplicationServer server = null;

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                server = prestoWcf.Service.GetServerById(serverId);
            }

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = server;
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveTask(string appId, string eTag, TaskDosCommand task)  // Couldn't use TaskBase as parameter: jQuery error: Cannot create an abstract class
        {
            // Notes about this method's parameters:
            //   We can't accept a parameter this is abstract (TaskBase) or a parameter that has abstract
            //   types as properties (App has List<TaskBase>). This is because JSON.stringify can't
            //   serialize these types. This is my exact issue: http://stackoverflow.com/q/5861241/279516
            // Two options:
            //   1. Use a concrete type as action parameter
            //   2. Write a custom model binder for this abstract class which based on some request parameters will return a specific instance.

            // Due to the above issue, we just take the app ID and eTag.

            // Get the app
            Application app;
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                app = prestoWcf.Service.GetById(appId);
            }

            // If the etag is different, don't even bother trying to save because we know the app has already been modified.
            if (app.Etag.ToString() != eTag)
            {
                // ToDo: What? Throw exception?
            }

            // Now update it with the updated task. Since tasks don't have an ID, use sequence. Is this dangerous?
            var taskDosCommand           = app.Tasks.First(x => x.Sequence == task.Sequence) as TaskDosCommand;
            taskDosCommand.Description   = task.Description;
            taskDosCommand.DosExecutable = task.DosExecutable;
            taskDosCommand.Parameters    = task.Parameters;

            // And save the app
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                app = prestoWcf.Service.SaveApplication(app);
            }

            // Just return the task for now, to test that the call is working.
            // Note, we really should be saving the entire app.
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = app;
            return jsonResult;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
