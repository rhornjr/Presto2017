using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.SqlServer
{
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        public IEnumerable<ApplicationServer> GetAll()
        {
            return this.Database.ApplicationServers.ToList();
        }

        public ApplicationServer GetByName(string serverName)
        {
            throw new NotImplementedException();
        }

        public ApplicationServer GetById(string id)
        {
            throw new NotImplementedException();
        }

        public void Save(ApplicationServer applicationServer)
        {
            this.SaveChanges<ApplicationServer>(applicationServer);
        }
    }
}
