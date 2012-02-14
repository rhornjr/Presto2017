using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;
using System.Xml.Serialization;
using PrestoServerCommon;
using PrestoServerCommon.Interfaces;

namespace SelfUpdatingServiceHost
{
    public class UpdaterController : IDisposable
    {
        private bool _initialRunningOfAppOccurred;
        private AppDomain _appDomain;
        private string _appName;
        private string _runningAppPath;
        private string _fullyQualifiedClassName;
        private string _sourceBinaryPath;
        private Timer _timer;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private IStartStop _startStopControllerToRun;

        private static readonly object _locker = new object();

        public void Start()
        {
            InitializeVariables();

            int checkForNewBinariesInterval = Convert.ToInt32(ConfigurationManager.AppSettings["CheckForNewBinariesInterval"], CultureInfo.InvariantCulture);

            this._timer = new Timer(this.Process, this._autoResetEvent, 0, checkForNewBinariesInterval);
        }

        public void Stop()
        {
            ServerCommonLogUtility.LogInformation("Stopping timer in UpdaterController and stopping IStartStop app.");
            if (this._timer != null) { this._timer.Dispose(); }
            StopApp();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UpdaterController")]
        private void InitializeVariables()
        {
            this._appName                 = ConfigurationManager.AppSettings["AppName"];
            string thisServicePath        = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this._runningAppPath          = Path.Combine(thisServicePath, this._appName);
            this._fullyQualifiedClassName = ConfigurationManager.AppSettings["FullyQualifiedClassName"];
            this._sourceBinaryPath        = ConfigurationManager.AppSettings["SourceBinaryPath"];

            ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "UpdaterController initialized." + Environment.NewLine +
                "App name: {0}" + Environment.NewLine +
                "Running path: {1}" + Environment.NewLine +
                "Source binary path: {2}",
                this._appName,
                this._runningAppPath,
                this._sourceBinaryPath));
        }

        private void Process(object stateInfo)
        {
            if (!Monitor.TryEnter(_locker)) { return; }

            try
            {
                // Check for new binaries. If new, copy the files and restart the app domain.            
                UpdaterManifest updaterManifest = GetUpdaterManifest();

                if (updaterManifest == null) { return; }

                if (VersionHasBeenInstalled(updaterManifest))
                {
                    IfAppNotRunningThenRunApp();
                    return;
                }                

                DeleteFiles();
                CopyFiles();

                UpdateMostRecentlyInstalledVersionInAppConfig(updaterManifest);
                this._initialRunningOfAppOccurred = true;
                RestartAppDomain();
            }
            catch (Exception ex)
            {
                // Just log and keep trying to process (ie: don't stop timer).
                ServerCommonLogUtility.LogException(ex);                
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private void IfAppNotRunningThenRunApp()
        {
            if (this._initialRunningOfAppOccurred) { return; }

            ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "Loading and running {0} for the first time...",
                this._appName));

            StartApp();

            this._initialRunningOfAppOccurred = true;
        }        

        private UpdaterManifest GetUpdaterManifest()
        {
            string filePathAndName = Path.Combine(this._sourceBinaryPath, this._appName + ".UpdaterManifest");

            if (!File.Exists(filePathAndName))
            {
                ServerCommonLogUtility.LogWarning(string.Format(CultureInfo.CurrentCulture,
                    "The updater manifest file was not found. This file is necessary for the program to run: {0}",
                    filePathAndName));
                return null;
            }

            UpdaterManifest updaterManifest;

            try
            {
                // From: http://stackoverflow.com/questions/3709104/read-file-which-is-in-use
                // The FileAccess specifies what YOU want to do with the file. The FileShare specifies what OTHERS can do with the
                // file while you have it in use.
                using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdaterManifest));
                    updaterManifest = xmlSerializer.Deserialize(fileStream) as UpdaterManifest;
                }
            }
            catch (Exception ex)
            {
                // This usually means the manifest file was locked, so return null and we'll just try again at the next interval.
                ServerCommonLogUtility.LogException(ex);
                return null;
            }

            return updaterManifest;
        }

        private bool VersionHasBeenInstalled(UpdaterManifest updaterManifest)
        {
            string mostRecentlyInstalledVersion = ConfigurationManager.AppSettings["MostRecentlyInstalledVersion"];

            if (updaterManifest.Version != mostRecentlyInstalledVersion)
            {
                ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                    "New version ({0}) of {1} detected. Old version was {2}. App will be updated and restarted.",
                    updaterManifest.Version,
                    this._appName,
                    mostRecentlyInstalledVersion));

                return false;
            }

            return true;
        }

        private static void UpdateMostRecentlyInstalledVersionInAppConfig(UpdaterManifest updaterManifest)
        {
            // This doesn't work when running within the VS debugger. This is because VS updates the vshost.exe.config.

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["MostRecentlyInstalledVersion"].Value = updaterManifest.Version;

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        private void RestartAppDomain()
        {
            StopApp();
            StartApp();
        }        

        private void CopyFiles()
        {
            if (!Directory.Exists(this._runningAppPath))
            {
                ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                    "Running app path did not exist. Creating it: {0}",
                    this._runningAppPath));
                Directory.CreateDirectory(this._runningAppPath);
            }

            ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "Copying files." + Environment.NewLine +
                "From: {0}" + Environment.NewLine +
                "To: {1}",
                this._sourceBinaryPath,
                this._runningAppPath));

            foreach (string file in Directory.GetFiles(this._sourceBinaryPath))
            {
                string fileName        = Path.GetFileName(file);
                string destinationFile = Path.Combine(this._runningAppPath, fileName);
                File.Copy(file, destinationFile, true);
            }
        }

        private void DeleteFiles()
        {
            if (!Directory.Exists(this._runningAppPath)) { return; }

            ServerCommonLogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "Deleting all files in {0}",
                this._runningAppPath));

            foreach (string file in Directory.GetFiles(this._runningAppPath))
            {
                File.Delete(file);
            }
        }

        private void StartApp()
        {
            if (!Directory.Exists(this._runningAppPath)) { CopyFiles(); }

            string configFile   = Path.Combine(_runningAppPath, this._appName + ".exe.config");
            string cachePath    = Path.Combine(_runningAppPath, "_cache");
            string assemblyName = Path.Combine(_runningAppPath, this._appName + ".exe");

            AppDomainSetup appDomainSetup    = new AppDomainSetup();
            appDomainSetup.ApplicationName   = this._appName;
            appDomainSetup.ShadowCopyFiles   = "true";  // Note: not a bool.
            appDomainSetup.CachePath         = cachePath;
            appDomainSetup.ConfigurationFile = configFile;
            
            // We need this when we run the app domain. This allows us to have the self-updating service host *NOT* reference PrestoCommon.
            //appDomainSetup.PrivateBinPath    = this._runningAppPath;  // This works too. Can fall back to this if there are any issues with ApplicationBase.
            appDomainSetup.ApplicationBase = this._runningAppPath;
            
            this._appDomain = AppDomain.CreateDomain(this._appName, AppDomain.CurrentDomain.Evidence, appDomainSetup);
            this._startStopControllerToRun = (IStartStop)this._appDomain.CreateInstanceFromAndUnwrap(assemblyName, this._fullyQualifiedClassName);
            this._startStopControllerToRun.CommentFromServiceHost =
                "Service host file version " + PrestoServerCommonUtility.GetFileVersion(Assembly.GetExecutingAssembly());
            ServerCommonLogUtility.LogInformation("Created app domain.");

            this._startStopControllerToRun.Start();
            ServerCommonLogUtility.LogInformation("Started app.");
        }

        private void StopApp()
        {
            if (this._startStopControllerToRun != null)
            {
                try
                {
                    this._startStopControllerToRun.Stop();
                }
                catch (RemotingException)
                {
                    // Do nothing. If a controller has a lease that expires, we'll get an exception when we try
                    // to access it. If that happens, just log it and move on. We still want to unload the app.
                    ServerCommonLogUtility.LogWarning("An attempt was made to stop an IStartStop controller but a remoting " +
                        "exception occurred. This may be because the controller had its lease expire. We'll just " +
                        "log this event and move on. We still want to unload the app domain and try to continue.");
                }
                ServerCommonLogUtility.LogInformation("Stopped app.");
            }

            if (this._appDomain != null)
            {                
                // If we don't do this, the old app domain keeps running.
                AppDomain.Unload(this._appDomain);
                ServerCommonLogUtility.LogInformation("Unloaded app domain.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing == false) { return; }

            if (this._timer != null) { this._timer.Dispose(); }

            if (this._autoResetEvent != null) { this._autoResetEvent.Dispose(); }
        }
    }
}
