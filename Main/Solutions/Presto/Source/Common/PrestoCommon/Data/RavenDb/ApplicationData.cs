using System.Collections.Generic;
using System.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;
using Raven.Client;

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
            return QueryAndCacheEtags(session => session.Query<Application>()).Cast<Application>();
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Application GetByName(string name)
        {
            return QuerySingleResultAndCacheEtag(session => session.Query<Application>()
                .Where(app => app.Name == name).FirstOrDefault())
                as Application;
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Application GetById(string id)
        {
            using (IDocumentSession session = Database.OpenSession())
            {
                return session.Query<Application>().Where(app => app.Id == id).FirstOrDefault();
            }
        }

        /// <summary>
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Save(Application application)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Save(application);
        }
    }
}
