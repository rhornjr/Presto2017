using System;
using System.Linq;
using System.Threading;
using System.Timers;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;
using PrestoCommon.Misc;

namespace PrestoTaskRunner.Logic
{
    /// <summary>
    /// Main controller for the logic within the presto task runner service
    /// </summary>
    public class PrestoTaskRunnerController : IDisposable
    {
        private System.Timers.Timer _timer;
        private static readonly object _locker = new object();

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Initialize();
            CheckForApplicationsToInstall();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this._timer.Stop();            
        }

        private void Initialize()
        {
            this._timer = new System.Timers.Timer(60000);
            this._timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);            
            this._timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckForApplicationsToInstall();
        }

        private static void CheckForApplicationsToInstall()
        {
            if (!Monitor.TryEnter(_locker)) { return; }  // Don't let  multiple threads in here at the same time.

            try
            {
                // Find an application that needs to be installed.            

                // Step 1: Get the app server, on which this process is running, from the DB.

                string thisServerName = Environment.MachineName;

                IObjectContainer db = CommonUtility.GetDatabase();

                ApplicationServer appServer = (from ApplicationServer server in db
                                               where server.Name == thisServerName
                                               select server).FirstOrDefault();

                if (appServer == null)
                {
                    // ToDo: Log warning here.
                }

                // Step 2: Get the applications associated with this server.


                // Step 3: Get the list of InstallationStatus entities to validate against our list of apps.
                //         If we find an app that needs to be installed, install it.
                InstallApplication(new Application());
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private static InstallationStatus InstallApplication(Application application)
        {
            // ToDo: Implement this
            return new InstallationStatus() { Application = application };
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing == false) { return; }

            if (this._timer != null) { this._timer.Dispose(); }
        }
    }
}
