using System.Collections.Generic;
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

        public IEnumerable<Application> GetAllApplications()
        {
            return ApplicationLogic.GetAll();
        }
    }
}
