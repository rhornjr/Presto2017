using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client.Linq;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationData : DataAccessLayerBase, IApplicationData
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Application> GetAll()
        {
            return ExecuteQuery<IEnumerable<Application>>(() =>
                QueryAndCacheEtags(session => session.Query<Application>()).Cast<Application>());
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Application GetByName(string name)
        {
            return ExecuteQuery<Application>(() =>
                QuerySingleResultAndCacheEtag(session => session.Query<Application>()
                .Where(app => app.Name == name).FirstOrDefault())
                as Application);
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Application GetById(string id)
        {
            return ExecuteQuery<Application>(() =>
                QuerySingleResultAndCacheEtag(session => session.Query<Application>()
                .Where(app => app.Id == id).FirstOrDefault())
                as Application);
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="appIds">The app ids.</param>
        /// <returns></returns>
        public IEnumerable<Application> GetByIds(IEnumerable<string> appIds)
        {
            return ExecuteQuery<IEnumerable<Application>>(() =>
                QueryAndCacheEtags(
                session => session.Query<Application>()
                .Where(app => app.Id.In<string>(appIds))).Cast<Application>());
        }

        /// <summary>
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Save(Application application)
        {
            new GenericData().Save(application);
        }
    }
}
