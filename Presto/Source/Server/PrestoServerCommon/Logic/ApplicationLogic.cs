using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;

namespace PrestoServer.Logic
{
    public static class ApplicationLogic
    {
        public static IEnumerable<Application> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetAll();
        }

        public static Application GetByName(string name)
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetByName(name);
        }

        public static Application GetById(string id)
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetById(id);
        }

        public static void Save(Application application)
        {
            try
            {
                DataAccessFactory.GetDataInterface<IApplicationData>().Save(application);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, application.Name);
                throw;
            }
        }
    }
}
