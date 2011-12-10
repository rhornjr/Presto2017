using System.Collections.Generic;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableGroupLogic : LogicBase
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CustomVariableGroup> GetAll()
        {
            IEnumerable<CustomVariableGroup> groups = from CustomVariableGroup customGroup in Database
                                                      select customGroup;

            Database.Ext().Refresh(groups, 10);

            return groups;
        }
    }
}
