using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Timers;
using PrestoCommon.Entities;
using PrestoCommon.Misc;
using PrestoServer;
using PrestoServer.Logic;
using Xanico.Core;
using Xanico.Core.Interfaces;

namespace PrestoTaskRunner.Logic
{
    /// <summary>
    /// Main controller for the logic within the presto task runner service
    /// </summary>
    public class PrestoTaskRunnerController : MarshalByRefObject, IStartStop, IDisposable
    {
        private string _commentFromServiceHost = string.Empty;
        private System.Timers.Timer _timer;
        private static readonly object _locker = new object();

        internal const string PrestoTaskRunnerName = "Presto Task Runner";

        /// <summary>
        /// Gets or sets the comment from service host. This comment is displayed in a ping response. It's typically
        /// used for the service host to pass in its file version, so it can be displayed with the ping response.
        /// </summary>
        /// <value>
        /// The comment from service host.
        /// </value>
        public string CommentFromServiceHost
        {
            get { return this._commentFromServiceHost; }

            set
            {
                this._commentFromServiceHost = value;
            }
        }

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
            Logger.LogInformation("PrestoTaskRunnerController stopping timer.", PrestoTaskRunnerName);
            this._timer.Stop();
            Thread.Sleep(2000);  // HACK: Give threads a chance to complete before the self-updating service unloads this app domain.
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
            Utility.SetLoggerSource();

            PrestoServerUtility.RegisterRavenDataClasses();
            PrestoServerUtility.RegisterRealClasses();

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

        private void AnswerPingRequest()
        {
            try
            {
                PingRequest pingRequest = PingRequestLogic.GetMostRecent();

                if (pingRequest == null) { return; }

                ApplicationServer appServer = GetApplicationServerForThisMachine(Environment.MachineName);

                // Can't do anything if we don't have an app server
                if (appServer == null) { return; }

                // See if we have already responded to the most recent ping request.

                PingResponse pingResponse = PingResponseLogic.GetByPingRequestAndServer(pingRequest, appServer);

                if (pingResponse != null) { return; }  // Already responded.

                string comment = "PTR file version " + ReflectionUtility.GetFileVersion(Assembly.GetExecutingAssembly()) + " -- " + this.CommentFromServiceHost;

                pingResponse = new PingResponse(pingRequest.Id, DateTime.Now, appServer, comment);

                PingResponseLogic.Save(pingResponse);

                Logger.LogInformation(string.Format(CultureInfo.CurrentCulture,
                    "{0} responded to ping request", appServer.Name), PrestoTaskRunnerName);
            }
            catch (Exception ex)
            {
                // Just eat it. We don't want ping response failures to stop processing.
                CommonUtility.ProcessException(ex, PrestoTaskRunnerName);
            }
        }

        private static void CheckForApplicationsToInstall()
        {
            try
            {
                ApplicationServer appServer = GetApplicationServerForThisMachine(Environment.MachineName);

                if (appServer == null) { return; }

                ApplicationServerLogic.InstallApplications(appServer);
            }
            catch (Exception ex)
            {
                // Log it and keep processing.
                CommonUtility.ProcessException(ex, PrestoTaskRunnerName);
            }
        }

        private static ApplicationServer GetApplicationServerForThisMachine(string serverName)
        {
            // Get the app server, on which this process is running.

            ApplicationServer appServer = ApplicationServerLogic.GetByName(serverName);

            if (appServer == null)
            {
                Logger.LogWarning(string.Format(CultureInfo.CurrentCulture,
                    PrestoTaskRunnerResources.AppServerNotFound,
                    serverName),
                    PrestoTaskRunnerName);
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
