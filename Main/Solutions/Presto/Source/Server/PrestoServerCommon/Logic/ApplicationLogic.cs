using System;
using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;

namespace PrestoServer.Logic
{
    public static class ApplicationLogic
    {
        public static IEnumerable<Application> GetAll(bool includeArchivedApps)
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetAll(includeArchivedApps);
        }

        public static IEnumerable<Application> GetAllSlim()
        {
            return DataAccessFactory.GetDataInterface<IApplicationData>().GetAllSlim();
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
            if (application == null) { throw new ArgumentNullException("application"); }

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
