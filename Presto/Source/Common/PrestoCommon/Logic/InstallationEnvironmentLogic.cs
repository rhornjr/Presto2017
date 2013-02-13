using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
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
