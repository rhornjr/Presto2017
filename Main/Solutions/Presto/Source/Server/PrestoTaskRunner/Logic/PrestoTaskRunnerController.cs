using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Timers;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Logic;
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
                string serverName = Environment.MachineName;

                ApplicationServer appServer = GetApplicationServerForThisMachine(serverName);

                if (appServer == null) { return; }

                // Get the list of InstallationStatus entities to validate against our list of apps.                
                IEnumerable<InstallationSummary> installationSummaryList = InstallationSummaryLogic.GetByServerName(serverName);

                InstallApplications(appServer, installationSummaryList);            
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private static void InstallApplications(ApplicationServer appServer, IEnumerable<InstallationSummary> installationSummaryList)
        {
            // If we find an app that needs to be installed, install it.
            foreach (Application app in appServer.Applications)
            {
                if (app.ForceInstallation || !InstallationSummaryFoundForApplication(installationSummaryList, app))
                {
                    // No installation summary found for this app, so install it (or we're forcing an installation).
                    LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                        PrestoTaskRunnerResources.AppWillBeInstalledOnAppServer,
                        app.Name,
                        appServer.Name));

                    InstallApplication(app, appServer);
                }
            }
        }

        private static bool InstallationSummaryFoundForApplication(IEnumerable<InstallationSummary> installationSummaryList, Application app)
        {
            return installationSummaryList.Where(summary => summary.Application.Name == app.Name).FirstOrDefault() != null;
        }

        private static ApplicationServer GetApplicationServerForThisMachine(string serverName)
        {
            // Get the app server, on which this process is running.

            ApplicationServer appServer = ApplicationServerLogic.GetByName(serverName);

            if (appServer == null)
            {
                LogUtility.LogWarning(string.Format(CultureInfo.CurrentCulture,
                    PrestoTaskRunnerResources.AppServerNotFound,
                    appServer.Name));
            }

            return appServer;
        }

        private static void InstallApplication(Application application, ApplicationServer appServer)
        {
            InstallationSummary installationSummary = new InstallationSummary(application, appServer, DateTime.Now);

            installationSummary.InstallationResult = ProcessTasks(application);

            installationSummary.InstallationEnd    = DateTime.Now;

            LogInstallationSummary(installationSummary);
        }

        private static void LogInstallationSummary(InstallationSummary installationSummary)
        {
            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                PrestoTaskRunnerResources.ApplicationInstalled,
                installationSummary.Application.Name,
                installationSummary.ApplicationServer.Name,
                installationSummary.InstallationStart.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationEnd.ToString(CultureInfo.CurrentCulture),
                installationSummary.InstallationResult.ToString()));
        }

        private static InstallationResult ProcessTasks(Application application)
        {
            bool atLeastOneTaskFailed = false;
            int numberOfSuccessfulTasks = 0;

            foreach (TaskBase task in application.Tasks)
            {
                task.Execute();

                if (task.TaskSucceeded == true) { numberOfSuccessfulTasks++; }

                if (task.TaskSucceeded == false)
                {
                    atLeastOneTaskFailed = true;
                    if (task.FailureCausesAllStop == 1) { break; }  // No more processing.
                }
            }

            if (numberOfSuccessfulTasks < 1) { return InstallationResult.Failure; }

            if (atLeastOneTaskFailed) { return InstallationResult.PartialSuccess; }

            return InstallationResult.Success;
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
