using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationWithOverrideVariableGroupLogic
    {
        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        public static ApplicationWithOverrideVariableGroup GetByAppNameAndGroupName(string appName, string groupName)
        {
            return DataAccessFactory.GetDataInterface<IApplicationWithOverrideVariableGroupData>().GetByAppNameAndGroupName(appName, groupName);
        }
    }
}
