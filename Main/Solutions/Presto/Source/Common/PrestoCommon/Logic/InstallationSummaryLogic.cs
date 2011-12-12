﻿using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationSummaryLogic : LogicBase
    {
        /// <summary>
        /// Gets the name of the by server.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public static IEnumerable<InstallationSummary> GetByServerName(string serverName)
        {
            IEnumerable<InstallationSummary> installationSummaryList = from InstallationSummary summary in Database
                                                                       where summary.ApplicationServer.Name.ToUpperInvariant()
                                                                         == serverName.ToUpperInvariant()
                                                                       select summary;

            Database.Ext().Refresh(installationSummaryList, 10);

            return installationSummaryList;
        }

        /// <summary>
        /// Gets the most recent by start time.
        /// </summary>
        /// <param name="numberToRetrieve">The number to retrieve.</param>
        /// <returns></returns>
        public static IEnumerable<InstallationSummary> GetMostRecentByStartTime(int numberToRetrieve)
        {
            IEnumerable<InstallationSummary> installationSummaryList = (from InstallationSummary summary in Database
                                                                        orderby summary.InstallationStart descending
                                                                        select summary).Take(numberToRetrieve);

            Database.Ext().Refresh(installationSummaryList, 10);

            return installationSummaryList;
        }
    }
}
