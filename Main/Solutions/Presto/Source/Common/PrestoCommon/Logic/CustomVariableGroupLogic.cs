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
        /// Gets the CustomVariableGroup by the specified application name.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <returns></returns>
        public static CustomVariableGroup Get(string applicationName)
        {
            return DataAccessFactory.GetDataInterface<ICustomVariableGroupData>().GetByName(applicationName);
        }
    }
}
