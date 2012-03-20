using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICustomVariableGroupData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<CustomVariableGroup> GetAll();

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        CustomVariableGroup GetByName(string name);

        /// <summary>
        /// Gets the object by ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns></returns>
        CustomVariableGroup GetById(string id);

        /// <summary>
        /// Saves the specified custom variable group.
        /// </summary>
        /// <param name="customVariableGroup">The custom variable group.</param>
        void Save(CustomVariableGroup customVariableGroup);
    }
}
