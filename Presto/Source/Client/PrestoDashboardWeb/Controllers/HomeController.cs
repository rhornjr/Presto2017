using System.Linq;
using System.Web.Mvc;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoDashboardWeb.Models;

namespace PrestoDashboardWeb.Controllers
{
    public class HomeController : Controller
    {
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

            return View(container);
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
        public ActionResult SaveTask(string app, TaskDosCommand task)  // Couldn't use TaskBase as parameter: jQuery error: Cannot create an abstract class
        {
            // Note: When app is of type string, the parameter is null. I believe this is because the call to this
            //       method uses JSON.stringify() and that can't create the abstract TaskBase List<TaskBase>.

            // This is my exact issue: http://stackoverflow.com/q/5861241/279516
            // Two options:
            //   1. Use a concrete type as action parameter
            //   2. Write a custom model binder for this abstract class which based on some request parameters will return a specific instance.

            // Another option is to NOT have an app parameter, but just take the app ID and eTag.

            // Just return the task for now, to test that the call is working.
            // Note, we really should be saving the entire app.
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = task;
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
