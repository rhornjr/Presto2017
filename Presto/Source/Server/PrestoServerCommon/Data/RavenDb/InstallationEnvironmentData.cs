﻿using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Data.RavenDb
{
    public class InstallationEnvironmentData : DataAccessLayerBase, IInstallationEnvironmentData
    {
        public IEnumerable<InstallationEnvironment> GetAll()
        {
            return ExecuteQuery<IEnumerable<InstallationEnvironment>>(() =>
            {
                IEnumerable<InstallationEnvironment> items = QueryAndSetEtags(session =>
                    session.Query<InstallationEnvironment>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Take(int.MaxValue)
                    ).AsEnumerable().Cast<InstallationEnvironment>();

                return items;
            });
        }

        public void Save(InstallationEnvironment env)
        {
            if (env == null) { throw new ArgumentNullException("env"); }

            new GenericData().Save(env);
        }
    }
}
