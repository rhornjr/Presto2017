using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface IApplicationData
    {
        IEnumerable<Application> GetAll(bool includeArchivedApps);

        Application GetByName(string name);

        Application GetById(string id);

        void Save(Application application);
    }
}
