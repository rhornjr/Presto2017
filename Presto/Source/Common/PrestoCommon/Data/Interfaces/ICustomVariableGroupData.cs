﻿using System.Collections.Generic;
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
        /// Gets the specified application name.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <returns></returns>
        CustomVariableGroup GetByApplication(Application application);

        /// <summary>
        /// Saves the specified custom variable group.
        /// </summary>
        /// <param name="customVariableGroup">The custom variable group.</param>
        void Save(CustomVariableGroup customVariableGroup);
    }
}