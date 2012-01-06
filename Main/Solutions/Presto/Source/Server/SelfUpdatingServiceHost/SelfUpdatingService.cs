using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;

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
                LogUtility.LogException(ex);

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
    }
}
