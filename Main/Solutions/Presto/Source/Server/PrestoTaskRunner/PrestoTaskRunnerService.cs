using System;
using System.ServiceProcess;
using PrestoCommon.Misc;
using PrestoTaskRunner;
using PrestoTaskRunner.Logic;

namespace PrestoTaskProcessor
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PrestoTaskRunnerService : ServiceBase
    {
        private static PrestoTaskRunnerService _prestoTaskRunnerService;
        private PrestoTaskRunnerController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrestoTaskRunnerService"/> class.
        /// </summary>
        public PrestoTaskRunnerService()
        {
            InitializeComponent();
        }

        #region [Main]

        private static void Main(string[] args)
        {
            try
            {
                _prestoTaskRunnerService = new PrestoTaskRunnerService();

                // Can't run this as a service because the outer service (SelfUpdatingServiceHost) is already running as a service.
                // If we try to run this as a service, we get this error:
                // Service cannot be started. An instance of the service is already running
                //if (!Environment.UserInteractive)
                //{
                //    Run(_prestoTaskRunnerService);
                //    return;
                //}

                // Run service as console app
                _prestoTaskRunnerService.OnStart(args);
                Console.WriteLine(PrestoTaskRunnerResources.ServerStartedConsoleMessage);
                Console.ReadKey();                
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);

                if (Environment.UserInteractive)
                {
                    Console.ReadKey();
                }
            }
            finally
            {
                _prestoTaskRunnerService.OnStop();
            }
        }

        #endregion

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            this._controller = new PrestoTaskRunnerController();

            _controller.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            this._controller.Stop();
        }
    }
}
