using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceProcess;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoServer;
using PrestoServer.Data.RavenDb;
using PrestoWcfService.SignalR;
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
            
            //Console.ReadKey();
            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<PrestoHub>();
            //hubContext.Clients.All.OnSignalRMessage("snuh");
            //Console.WriteLine("Sent 'snuh' to all clients...");

            //Console.ReadKey();
            //hubContext.Clients.All.OnDatabaseItemAdded("db item added");
            //Console.WriteLine("DB item added...");

            //Console.ReadKey();
            //hubContext.Clients.All.OnSignalRMessage("snuh2");
            //Console.WriteLine("Sent 'snuh' to all clients...");
            
            Console.ReadKey();

            _prestoServiceHost.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            RegisterDependencies();
            InitializeAndOpenPrestoService();
            StartSignalRHost();
            SubscribeToDatabaseChangeEvents();
        }

        private void SubscribeToDatabaseChangeEvents()
        {
            // When there is a new installation summary, automatically refresh the list.
            DataAccessLayerBase.NewInstallationSummaryAddedToDb += OnDatabaseItemAdded;
        }

        private void OnDatabaseItemAdded(object sender, EventArgs<string> e)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<PrestoHub>();
            hubContext.Clients.All.OnDatabaseItemAdded("snuh");
        }

        private void StartSignalRHost()
        {
            var url = ConfigurationManager.AppSettings["signalrAddress"];
            WebApplication.Start<Startup>(url);
        }

        private void InitializeAndOpenPrestoService()
        {
            if (_serviceHost != null) { _serviceHost.Close(); }

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

        private void RegisterDependencies()
        {
            PrestoServerUtility.RegisterRavenDataClasses();
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
