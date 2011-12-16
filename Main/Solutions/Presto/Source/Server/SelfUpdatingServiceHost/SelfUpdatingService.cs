using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace SelfUpdatingServiceHost
{
    public partial class SelfUpdatingService : ServiceBase
    {
        public SelfUpdatingService()
        {
            InitializeComponent();
        }

        #region [Main]

        private static void Main(string[] args)
        {
            SelfUpdatingService selfUpdatingService = new SelfUpdatingService();

            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                if (Environment.UserInteractive)
                {
                    Console.ReadKey();
                }
            }
            finally
            {
                selfUpdatingService.OnStop();
            }
        }

        #endregion

        protected override void OnStart(string[] args)
        {
            UpdaterController updaterController = new UpdaterController();

            updaterController.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
