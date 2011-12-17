using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.Xml.Serialization;

namespace SelfUpdatingServiceHost
{
    public class UpdaterController
    {
        private AppDomain _appDomain;
        private string _appName;
        private string _runningAppPath;
        private string _sourceBinaryPath;
        private Timer _timer;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private static readonly object _locker = new object();

        public void Start()
        {
            this._appName          = ConfigurationManager.AppSettings["AppName"];
            string thisServicePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this._runningAppPath   = Path.Combine(thisServicePath, this._appName);
            this._sourceBinaryPath = ConfigurationManager.AppSettings["SourceBinaryPath"];

            //DeleteFiles();
            CopyFiles();

            LoadApp();

            int checkForNewBinariesInterval = Convert.ToInt32(ConfigurationManager.AppSettings["CheckForNewBinariesInterval"]);

            this._timer = new Timer(this.Process, this._autoResetEvent, checkForNewBinariesInterval, checkForNewBinariesInterval);
        }

        private void Process(object stateInfo)
        {
            if (!Monitor.TryEnter(_locker)) { return; }

            try
            {
                // Check for new binaries. If new, copy the files and restart the app domain.            
                UpdaterManifest updaterManifest = GetUpdaterManifest();
                if (VersionHasBeenInstalled(updaterManifest)) { return; }

                DeleteFiles();
                CopyFiles();

                UpdateMostRecentlyInstalledVersion(updaterManifest);

                RestartAppDomain();
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }        

        private UpdaterManifest GetUpdaterManifest()
        {
            string filePathAndName = Path.Combine(this._sourceBinaryPath, this._appName + ".UpdaterManifest");

            if (!File.Exists(filePathAndName)) { return null; }

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
          
            return updaterManifest.Version == mostRecentlyInstalledVersion;
        }

        private void UpdateMostRecentlyInstalledVersion(UpdaterManifest updaterManifest)
        {
            // This doesn't work when running within the VS debugger. This is because VS updates the vshost.exe.config.

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["MostRecentlyInstalledVersion"].Value = updaterManifest.Version;

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        private void RestartAppDomain()
        {
            this._tokenSource.Cancel();
            //AppDomain.Unload(this._appDomain);
            LoadApp();
        }

        private void CopyFiles()
        {
            if (!Directory.Exists(this._runningAppPath))
            {
                Directory.CreateDirectory(this._runningAppPath);
            }

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

            string configFile = Path.Combine(_runningAppPath, this._appName + ".exe.config");
            string cachePath  = Path.Combine(_runningAppPath, "_cache");
            string assembly   = Path.Combine(_runningAppPath, this._appName + ".exe");

            AppDomainSetup appDomainSetup    = new AppDomainSetup();
            appDomainSetup.ApplicationName   = this._appName;
            appDomainSetup.ShadowCopyFiles   = "true";  // Note: not a bool.
            appDomainSetup.CachePath         = cachePath;
            appDomainSetup.ConfigurationFile = configFile;

            this._appDomain = AppDomain.CreateDomain(this._appName, AppDomain.CurrentDomain.Evidence, appDomainSetup);

            this._tokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    this._appDomain.ExecuteAssembly(assembly);
                }
                catch (AppDomainUnloadedException ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }, _tokenSource.Token);
        }
    }
}
