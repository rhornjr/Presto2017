using System.Collections.Generic;
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
    }
}
