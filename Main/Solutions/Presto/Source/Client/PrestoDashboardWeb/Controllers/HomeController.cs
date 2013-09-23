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
