﻿using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerLogic : LogicBase
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApplicationServer> GetAll()
        {
            IEnumerable<ApplicationServer> servers = from ApplicationServer server in Database
                                                     select server;

            return servers;
        }

        /// <summary>
        /// Gets an application server by name.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public static ApplicationServer GetByName(string serverName)
        {
            ApplicationServer appServer = (from ApplicationServer server in Database
                                           where server.Name.ToUpperInvariant() == serverName.ToUpperInvariant()
                                           select server).FirstOrDefault();            

            return appServer;
        }
    }
}