using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using PrestoServerCommon;

namespace SelfUpdatingServiceHost
{
    public partial class SelfUpdatingService : ServiceBase
    {
        UpdaterController _updaterController;

        public SelfUpdatingService()
        {
            InitializeComponent();
        }

        #region [Main]

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        private static void Main(string[] args)
        {            
            try
            {
                using (SelfUpdatingService selfUpdatingService = new SelfUpdatingService())
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
                ServerCommonLogUtility.LogException(ex);

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
            ServerCommonLogUtility.LogInformation("Stopping service.");

            if (this._updaterController == null) { return; }

            ServerCommonLogUtility.LogInformation("Calling Stop() and Dispose() on _updaterController.");

            this._updaterController.Stop();
            this._updaterController.Dispose();
        }
    }
}
