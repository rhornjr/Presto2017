using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Timers;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoServerCommon.Interfaces;

namespace PrestoTaskRunner.Logic
{
    /// <summary>
    /// Main controller for the logic within the presto task runner service
    /// </summary>
    public class PrestoTaskRunnerController : MarshalByRefObject, IStartStop, IDisposable
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
            int timerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["timerInterval"], CultureInfo.InvariantCulture);

            this._timer = new System.Timers.Timer(timerInterval);

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
                ApplicationServer appServer = GetApplicationServerForThisMachine(Environment.MachineName);

                if (appServer == null) { return; }

                appServer.InstallApplications();
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private static ApplicationServer GetApplicationServerForThisMachine(string serverName)
        {
            // Get the app server, on which this process is running.

            ApplicationServer appServer = ApplicationServerLogic.GetByName(serverName);

            if (appServer == null)
            {
                LogUtility.LogWarning(string.Format(CultureInfo.CurrentCulture,
                    PrestoTaskRunnerResources.AppServerNotFound,
                    serverName));
            }

            return appServer;
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
