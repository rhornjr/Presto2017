using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using Xanico.Core;

namespace SelfUpdatingServiceHost
{
    public partial class PrestoSelfUpdatingServiceHost : ServiceBase
    {
        UpdaterController _updaterController;

        public PrestoSelfUpdatingServiceHost()
        {
            InitializeComponent();
        }

        #region [Main]

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        private static void Main(string[] args)
        {            
            try
            {
                using (PrestoSelfUpdatingServiceHost selfUpdatingService = new PrestoSelfUpdatingServiceHost())
                {
                    if (!Environment.UserInteractive)
                    {
                        Run(selfUpdatingService);
                        return;
                    }

                    // Run service as console app
                    selfUpdatingService.OnStart(args);
                    Console.WriteLine("Service started. Press any key to exit.");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                if (Environment.UserInteractive)
                {
                    Console.ReadKey();
                }
            }
        }

        #endregion

        protected override void OnStart(string[] args)
        {
            _updaterController = new UpdaterController();
            _updaterController.Start();
        }

        protected override void OnStop()
        {
            Logger.LogInformation("Stopping service.");

            if (this._updaterController == null) { return; }

            Logger.LogInformation("Calling Stop() and Dispose() on _updaterController.");

            this._updaterController.Stop();
            this._updaterController.Dispose();
        }
    }
}
