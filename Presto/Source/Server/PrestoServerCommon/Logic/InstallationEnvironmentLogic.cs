using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Logic
{
    public static class InstallationEnvironmentLogic
    {
        public static IEnumerable<InstallationEnvironment> GetAll()
        {
            return DataAccessFactory.GetDataInterface<IInstallationEnvironmentData>().GetAll();
        }

        public static void Save(InstallationEnvironment environment)
        {
            DataAccessFactory.GetDataInterface<IInstallationEnvironmentData>().Save(environment);
        }
    }
}
