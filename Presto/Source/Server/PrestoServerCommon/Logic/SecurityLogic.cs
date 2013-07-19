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
    }
}
