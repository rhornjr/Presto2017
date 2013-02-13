using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    public interface IInstallationEnvironmentData
    {
        IEnumerable<InstallationEnvironment> GetAll();

        void Save(InstallationEnvironment environment);
    }
}
