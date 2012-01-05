using System.Collections.Generic;
using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class CustomVariableGroupLogic
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<CustomVariableGroup> GetAll()
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetAll();
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static CustomVariableGroup GetByName(string name)
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetByName(name);
        }

        /// <summary>
        /// Gets the CustomVariableGroup by the specified application name.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns></returns>
        public static CustomVariableGroup GetByApplication(Application application)
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetByApplication(application);
        }

        /// <summary>
        /// Saves the specified custom variable group.
        /// </summary>
        /// <param name="customVariableGroup">The custom variable group.</param>
        public static void Save(CustomVariableGroup customVariableGroup)
        {
            DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().Save(customVariableGroup);
        }
    }
}
