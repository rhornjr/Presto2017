using System.Collections.Generic;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationServerData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ApplicationServer> GetAll();

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="serverName">Name of the server.</param>
        /// <returns></returns>
        ApplicationServer GetByName(string serverName);

        /// <summary>
        /// Gets the object by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        ApplicationServer GetById(string id);

        /// <summary>
        /// Saves the specified application server.
        /// </summary>
        /// <param name="applicationServer">The application server.</param>
        void Save(ApplicationServer applicationServer);
    }
}
