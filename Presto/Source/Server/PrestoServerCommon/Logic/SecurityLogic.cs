using System;
using System.Collections.Generic;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Raven.Abstractions.Exceptions;

namespace PrestoServer.Logic
{
    public static class SecurityLogic
    {
        public static IEnumerable<AdGroupWithRoles> GetAll()
        {
            return DataAccessFactory.GetDataInterface<ISecurityData>().GetAll();
        }

        public static void Save(AdGroupWithRoles groupWithRoles)
        {
            if (groupWithRoles == null) { throw new ArgumentNullException("groupWithRoles"); }

            try
            {
                DataAccessFactory.GetDataInterface<ISecurityData>().Save(groupWithRoles);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, groupWithRoles.AdGroupName);
                throw;
            }
        }

        public static ActiveDirectoryInfo GetActiveDirectoryInfo()
        {
            return DataAccessFactory.GetDataInterface<ISecurityData>().GetActiveDirectoryInfo();
        }

        public static void SaveActiveDirectoryInfo(ActiveDirectoryInfo adInfo)
        {
            if (adInfo == null) { throw new ArgumentNullException("adInfo"); }

            try
            {
                DataAccessFactory.GetDataInterface<ISecurityData>().SaveActiveDirectoryInfo(adInfo);
            }
            catch (ConcurrencyException ex)
            {
                LogicBase.SetConcurrencyUserSafeMessage(ex, adInfo.Id);
                throw;
            }
        }
    }
}
