using System.Collections.Generic;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationLogic : LogicBase
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Application> GetAll()
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
        public static void Save(Application application)
        {
            Database.Store(application);
            Database.Commit();
        }
    }
}
