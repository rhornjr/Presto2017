using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    public interface ISecurityData
    {
        IEnumerable<AdGroupWithRoles> GetAll();
        void Save(AdGroupWithRoles groupWithRoles);
    }
}
