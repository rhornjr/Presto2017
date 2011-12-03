using System;
using System.Timers;
using PrestoCommon.Entities;

namespace PrestoTaskRunner.Logic
{
    /// <summary>
    /// Main controller for the logic within the presto task runner service
    /// </summary>
    public class PrestoTaskRunnerController : IDisposable
    {
        private Timer _timer;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            Initialize();
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
            this._timer = new Timer(60000);
            this._timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            this._timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            CheckForApplicationsToInstall();
        }

        private static void CheckForApplicationsToInstall()
        {
            // Find an application that needs to be installed.
            Application application = null;  // ToDo: Add DB call here to check for an app to install.

            InstallApplication(application);
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
