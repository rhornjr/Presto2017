using System.Collections.Generic;
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
        /// Saves the specified application.
        /// </summary>
        /// <param name="application">The application.</param>
        public void Save(Application application)
        {
            Database.Store(application);
            Database.Commit();
        }
    }
}
