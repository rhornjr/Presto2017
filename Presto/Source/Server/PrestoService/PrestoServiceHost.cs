using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceProcess;
using PrestoCommon.Interfaces;
using PrestoServer;
using PrestoWcfService.WcfServices;

namespace PrestoWcfService
{
    public partial class PrestoServiceHost : ServiceBase
    {
        private static PrestoServiceHost _prestoServiceHost = new PrestoServiceHost();
        private static ServiceHost _serviceHost = null;
        private static string _serviceAddress = ConfigurationManager.AppSettings["serviceAddress"];

        public PrestoServiceHost()
        {
            InitializeComponent();
        }

        private static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                Run(_prestoServiceHost);
                return;
            }

            // Run service as a console app
            _prestoServiceHost.OnStart(args);
            Console.WriteLine("Presto WCF service started: " + _serviceAddress);
            Console.WriteLine("Press any key to stop the program.");
            Console.ReadKey();
            _prestoServiceHost.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            if (_serviceHost != null) { _serviceHost.Close(); }

            PrestoServerUtility.RegisterRavenDataClasses();

            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;

            _serviceHost = new ServiceHost(typeof(PrestoService));
            _serviceHost.AddServiceEndpoint(typeof(IBaseService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(IApplicationService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(IServerService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(ICustomVariableGroupService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(IInstallationEnvironmentService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(IInstallationSummaryService), netTcpBinding, _serviceAddress);
            _serviceHost.AddServiceEndpoint(typeof(IPingService), netTcpBinding, _serviceAddress);

            _serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
        }
    }
}
