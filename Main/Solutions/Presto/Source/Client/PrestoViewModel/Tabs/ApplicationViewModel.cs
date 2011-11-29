using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;

namespace PrestoViewModel.Tabs
{
    public class ApplicationViewModel : ViewModelBase
    {
        private List<Application> _applications;

        public List<Application> Applications
        {
            get { return this._applications; }

            set
            {
                this._applications = value;
                this.NotifyPropertyChanged("Applications");
            }
        }

        public ApplicationViewModel()
        {
            LoadApplications();
        }

        private void LoadApplications()
        {
            IObjectContainer db = GetDatabase();            

            IEnumerable<Application> apps = from Application app in db
                                            select app;

            this.Applications = apps.ToList();

            db.Close();
        }

        private static IObjectContainer GetDatabase()
        {
            string databaseServerName = ConfigurationManager.AppSettings["databaseServerName"];
            string databaseUser       = ConfigurationManager.AppSettings["databaseUser"];
            string databasePassword   = ConfigurationManager.AppSettings["databasePassword"];
            int databaseServerPort    = Convert.ToInt32(ConfigurationManager.AppSettings["databaseServerPort"], CultureInfo.InvariantCulture);

            return Db4oClientServer.OpenClient(databaseServerName, databaseServerPort, databaseUser, databasePassword);
        }   
    }
}
