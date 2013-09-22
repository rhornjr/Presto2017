using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;

namespace PrestoDashboardWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var apps = new List<Application>();

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
                apps = prestoWcf.Service.GetAllApplications(true).ToList();
            }

            return View(apps);
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
