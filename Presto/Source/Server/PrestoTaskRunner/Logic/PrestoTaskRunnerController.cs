using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this._timer.Stop();            
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            // Returning null here will prevent the lease manager from deleting the object.
            // If we don't do this, then we get a remoting exception when calling Stop()
            // from the self-updating service host.
            return null;
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
            if (!Monitor.TryEnter(_locker)) { return; }  // Don't let  multiple threads in here at the same time.

            try
            {
                AnswerPingRequest();
                CheckForApplicationsToInstall();
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private static void AnswerPingRequest()
        {
            try
            {
                PingRequest pingRequest = PingRequestLogic.GetMostRecent();

                ApplicationServer appServer = GetApplicationServerForThisMachine(Environment.MachineName);

                // See if we have already responded to the most recent ping request.

                PingResponse pingResponse = PingResponseLogic.GetByPingRequestAndServer(pingRequest, appServer);

                if (pingResponse != null) { return; }  // Already responded.

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string comment = "PTR file version " + fileVersionInfo.ProductVersion;

                pingResponse = new PingResponse(pingRequest.Id, DateTime.Now, appServer, comment);

                PingResponseLogic.Save(pingResponse);

                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture, "{0} responded to ping request", appServer.Name));
            }
            catch (Exception ex)
            {
                // Just eat it. We don't want ping response failures to stop processing.
                LogUtility.LogException(ex);
            }
        }

        private static void CheckForApplicationsToInstall()
        {
            ApplicationServer appServer = GetApplicationServerForThisMachine(Environment.MachineName);

            if (appServer == null) { return; }

            appServer.InstallApplications();
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
