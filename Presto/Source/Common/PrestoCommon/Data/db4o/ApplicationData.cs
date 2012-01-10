using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Linq;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.db4o
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
            IEnumerable<Application> apps = from Application app in Database
                                            select app;

            Database.Ext().Refresh(apps, 10);

            return apps;
        }

        /// <summary>
        /// Gets the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Application GetByName(string name)
        {
            Application application = (from Application app in Database
                                       where app.Name == name
                                       select app).FirstOrDefault();

            Database.Ext().Refresh(application, 10);

            return application;
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Application GetById(string id)
        {
            // Only RavenDB needs this.
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets the by ids.
        /// </summary>
        /// <param name="appIds">The app ids.</param>
        /// <returns></returns>
        public IEnumerable<Application> GetByIds(IEnumerable<string> appIds)
        {
            throw new System.NotImplementedException();
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
