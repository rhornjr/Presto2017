using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Logic;

namespace PrestoWcfService.WcfServices
{
    public class PrestoService : IPrestoService
    {
        public string Echo(string message)
        {
            return message;
        }

        public List<Application> GetAllApplications()
        {
            IEnumerable<Application> apps = ApplicationLogic.GetAll();
            return apps.ToList();
        }
    }
}
