using System.Collections.Generic;
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
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        void Save(Application application);
    }
}
