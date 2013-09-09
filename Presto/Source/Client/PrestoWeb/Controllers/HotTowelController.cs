using System.Collections.Generic;
using System.Web.Mvc;
using PrestoCommon.Entities;

namespace PrestoWeb.Controllers
{
    public class HotTowelController : Controller
    {
        //
        // GET: /HotTowel/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetApps()
        {
            var apps = new List<Application>();

            apps.Add(new Application() { Name = "app4" });
            apps.Add(new Application() { Name = "app5" });

            //using (var prestoWcf = new PrestoWcf<IApplicationService>())
            //{
            //    apps = prestoWcf.Service.GetAllApplications(true).ToList();
            //}

            var jsonResult = new JsonResult();
            jsonResult.Data = apps;
            return jsonResult;
        }
    }
}
