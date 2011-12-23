using System;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using PrestoCommon.Misc;

namespace SelfUpdatingServiceHost
{
    public class UpdaterController : IDisposable
    {
        private bool _initialRunningOfAppOccurred;
        private AppDomain _appDomain;
        private string _appName;
        private string _runningAppPath;
        private string _sourceBinaryPath;
        private Timer _timer;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private CancellationTokenSource _tokenSource;

        private static readonly object _locker = new object();

        public void Start()
        {
            InitializeVariables();

            int checkForNewBinariesInterval = Convert.ToInt32(ConfigurationManager.AppSettings["CheckForNewBinariesInterval"], CultureInfo.InvariantCulture);

            this._timer = new Timer(this.Process, this._autoResetEvent, 0, checkForNewBinariesInterval);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UpdaterController")]
        private void InitializeVariables()
        {
            this._appName          = ConfigurationManager.AppSettings["AppName"];
            string thisServicePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this._runningAppPath   = Path.Combine(thisServicePath, this._appName);
            this._sourceBinaryPath = ConfigurationManager.AppSettings["SourceBinaryPath"];

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
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
                LogUtility.LogException(ex);
                if (this._timer != null) { this._timer.Dispose(); }
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }

        private void IfAppNotRunningThenRunApp()
        {
            if (this._initialRunningOfAppOccurred) { return; }

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "Loading and running {0} for the first time...",
                this._appName));

            LoadApp();

            this._initialRunningOfAppOccurred = true;
        }        

        private UpdaterManifest GetUpdaterManifest()
        {
            string filePathAndName = Path.Combine(this._sourceBinaryPath, this._appName + ".UpdaterManifest");

            if (!File.Exists(filePathAndName))
            {
                throw new FileNotFoundException("The updater manifest file was not found. This file is necessary for the program to run.", filePathAndName);
            }

            UpdaterManifest updaterManifest;

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UpdaterManifest));
                updaterManifest = xmlSerializer.Deserialize(fileStream) as UpdaterManifest;
            }

            return updaterManifest;
        }

        private bool VersionHasBeenInstalled(UpdaterManifest updaterManifest)
        {
            string mostRecentlyInstalledVersion = ConfigurationManager.AppSettings["MostRecentlyInstalledVersion"];

            if (updaterManifest.Version != mostRecentlyInstalledVersion)
            {
                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
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
            if (this._tokenSource != null) { this._tokenSource.Cancel(); }
            //AppDomain.Unload(this._appDomain);  // ToDo: In testing, this wasn't necessary. Should this still be done to keep things clean (ie: good clean-up)?
            LoadApp();
        }

        private void CopyFiles()
        {
            if (!Directory.Exists(this._runningAppPath))
            {
                LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                    "Running app path did not exist. Creating it: {0}",
                    this._runningAppPath));
                Directory.CreateDirectory(this._runningAppPath);
            }

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
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

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "Deleting all files in {0}",
                this._runningAppPath));

            foreach (string file in Directory.GetFiles(this._runningAppPath))
            {
                File.Delete(file);
            }
        }

        private void LoadApp()
        {
            // Load UpdatedConsoleApp in an app domain here.

            if (!Directory.Exists(this._runningAppPath))
            {
                CopyFiles();
            }

            string configFile   = Path.Combine(_runningAppPath, this._appName + ".exe.config");
            string cachePath    = Path.Combine(_runningAppPath, "_cache");
            string assemblyName = Path.Combine(_runningAppPath, this._appName + ".exe");

            AppDomainSetup appDomainSetup    = new AppDomainSetup();
            appDomainSetup.ApplicationName = this._appName;
            //appDomainSetup.ApplicationBase   = _runningAppPath;
            appDomainSetup.ShadowCopyFiles   = "true";  // Note: not a bool.
            appDomainSetup.CachePath         = cachePath;
            appDomainSetup.ConfigurationFile = configFile;

            //PermissionSet permissionSet = new PermissionSet(PermissionState.Unrestricted);
            //permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, this._sourceBinaryPath));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            //this._appDomain = AppDomain.CreateDomain(this._appName, AppDomain.CurrentDomain.Evidence, appDomainSetup, AppDomain.CurrentDomain.PermissionSet);
            this._appDomain = AppDomain.CreateDomain(this._appName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

            // ToDo: See comments at the bottom of this file for a better way to load/run/manage the app domain.            

            this._tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    this._appDomain.ExecuteAssembly(assemblyName);
                }
                catch (AppDomainUnloadedException ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }, _tokenSource.Token);
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

            if (this._tokenSource != null) { this._tokenSource.Dispose(); }
        }
    }
}

// ToDo: This may be the preferred approach (instead of this._appDomain.ExecuteAssembly(assemblyName);). If we instead use
//       CreateInstanceFromAndUnwrap(), then we have a proxy (the prestoTaskRunnerService instance) with which we can
//       talk. That means we can call Stop(), or other methods to tell the PTR to elegantly shut down. The one problem with
//       this is that this self-updating app must have a reference to the PTR. That's not generic and won't be usable that
//       way for other apps that want to automatically restart.
// Note: Didn't do this just yet because there are other priorities at the moment. Need to look into this.
// Note: http://stackoverflow.com/questions/88717/loading-dlls-into-a-separate-appdomain shows a short, clean example, and says
//       this: As far as I know PrestoTaskRunnerService has to inherit from MarshalByRefObject. That might be a problem,
//       because PrestoTaskRunnerService derives from ServiceBase, but PrestoTaskRunnerService shouldn't even be a service.
//       We'll need to change that to be a simple exe or console app.
//Type type = typeof(PrestoTaskRunnerService);
//PrestoTaskRunnerService prestoTaskRunnerService = (PrestoTaskRunnerService)this._appDomain.CreateInstanceFromAndUnwrap(assemblyName, type.FullName);
//prestoTaskRunnerService.Stop();