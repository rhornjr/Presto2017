using PrestoCommon.Entities;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrestoServer.Data.RavenDb.Indexes
{
    public class ApplicationArchived : AbstractIndexCreationTask<Application>
    {
        public ApplicationArchived()
        {
            Map = apps => from app in apps
                          select new { Archived = app.Archived };
        }
    }
}
