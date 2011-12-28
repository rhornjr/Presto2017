using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationWithOverrideVariableGroupData
    {
        /// <summary>
        /// Gets the name of the by app name and group.
        /// </summary>
        /// <param name="appName">Name of the app.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <returns></returns>
        ApplicationWithOverrideVariableGroup GetByAppNameAndGroupName(string appName, string groupName);
    }
}
