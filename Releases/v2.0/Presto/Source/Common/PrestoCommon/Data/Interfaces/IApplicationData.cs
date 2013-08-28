﻿using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Application> GetAll();

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Application GetByName(string name);

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        Application GetById(string id);

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="appIds">The app ids.</param>
        /// <returns></returns>
        IEnumerable<Application> GetByIds(IEnumerable<string> appIds);

        /// <summary>
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        void Save(Application application);
    }
}