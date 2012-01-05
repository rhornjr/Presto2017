using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerData : DataAccessLayerBase, IApplicationServerData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationServer> GetAll()
        {
            IEnumerable<ApplicationServer> appServers = QueryAndCacheEtags(session => 
                session.Advanced.LuceneQuery<ApplicationServer>()
                .Include(appServer => appServer.CustomVariableGroupIds)).Cast<ApplicationServer>();

            foreach (ApplicationServer appServer in appServers)
            {
                HydrateApplicationServer(appServer);
            }            

            return appServers;
        }        

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        public ApplicationServer GetByName(string serverName)
        {
            // Note: RavenDB queries are case-insensitive, so no ToUpper() conversion is necessary here.
            ApplicationServer appServer = QuerySingleResultAndCacheEtag(session => session.Query<ApplicationServer>()
                .Where(server => server.Name == serverName).FirstOrDefault())
                as ApplicationServer;

            if (appServer != null) { HydrateApplicationServer(appServer); }

            return appServer;
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="serverId">The server id.</param>
        /// <returns></returns>
        public ApplicationServer GetById(string serverId)
        {
            ApplicationServer appServer = QuerySingleResultAndCacheEtag(session => session.Query<ApplicationServer>()
                .Where(server => server.Id == serverId).FirstOrDefault())
                as ApplicationServer;

            if (appServer != null) { HydrateApplicationServer(appServer); }

            return appServer;
        }

        private static void HydrateApplicationServer(ApplicationServer appServer)
        {
            appServer.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>(
                DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetByIds(appServer.CustomVariableGroupIds));

            foreach (ApplicationWithOverrideVariableGroup appGroup in appServer.ApplicationsWithOverrideGroup)
            {
                appGroup.Application = DataAccessFactory.GetDataInterface<IApplicationData>().GetById(appGroup.ApplicationId);
                appGroup.CustomVariableGroup = DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetById(appGroup.CustomVariableGroupId);
            }

            if (appServer.ApplicationWithGroupToForceInstall != null)
            {
                appServer.ApplicationWithGroupToForceInstall.Application =
                    DataAccessFactory.GetDataInterface<IApplicationData>().GetById(appServer.ApplicationWithGroupToForceInstall.ApplicationId);
            }

            if (appServer.ApplicationWithGroupToForceInstall != null && appServer.ApplicationWithGroupToForceInstall.CustomVariableGroupId != null)
            {
                appServer.ApplicationWithGroupToForceInstall.CustomVariableGroup =
                    DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetById(appServer.ApplicationWithGroupToForceInstall.CustomVariableGroupId);
            }
        }

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        public void Save(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            // For each group, set its ApplicationId and CustomVariableGroupId.
            foreach (ApplicationWithOverrideVariableGroup appGroup in applicationServer.ApplicationsWithOverrideGroup)
            {
                appGroup.ApplicationId = appGroup.Application.Id;
                if (appGroup.CustomVariableGroup != null) { appGroup.CustomVariableGroupId = appGroup.CustomVariableGroup.Id; }
            }

            // For each group, add its ID to the ID list.
            applicationServer.CustomVariableGroupIds = new List<string>();
            foreach (CustomVariableGroup customGroup in applicationServer.CustomVariableGroups)
            {
                applicationServer.CustomVariableGroupIds.Add(customGroup.Id);
            }

            DataAccessFactory.GetDataInterface<IGenericData>().Save(applicationServer);
        }
    }
}
