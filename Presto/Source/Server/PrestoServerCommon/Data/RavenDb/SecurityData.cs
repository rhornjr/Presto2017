using System;
using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Entities;
using PrestoServer.Data.Interfaces;

namespace PrestoServer.Data.RavenDb
{
    public class SecurityData : DataAccessLayerBase, ISecurityData
    {
        public IEnumerable<AdGroupWithRoles> GetAll()
        {
            return ExecuteQuery<IEnumerable<AdGroupWithRoles>>(() =>
            {
                IEnumerable<AdGroupWithRoles> apps = QueryAndSetEtags(session =>
                    session.Query<AdGroupWithRoles>()
                    .Take(int.MaxValue))
                    .AsEnumerable().Cast<AdGroupWithRoles>();

                return apps;
            });
        }

        public void Save(AdGroupWithRoles groupWithRoles)
        {
            if (groupWithRoles == null) { throw new ArgumentNullException("groupWithRoles"); }

            new GenericData().Save(groupWithRoles);
        }

        public ActiveDirectoryInfo GetActiveDirectoryInfo()
        {
            return ExecuteQuery<ActiveDirectoryInfo>(() =>
            {
                return QuerySingleResultAndSetEtag(session =>
                    session.Query<ActiveDirectoryInfo>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .FirstOrDefault()) as ActiveDirectoryInfo;
            });
        }

        public void SaveActiveDirectoryInfo(ActiveDirectoryInfo adInfo)
        {
            if (adInfo == null) { throw new ArgumentNullException("adInfo"); }

            new GenericData().Save(adInfo);
        }
    }
}
