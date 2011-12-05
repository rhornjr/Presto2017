using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;
using Db4objects.Db4o;
using Db4objects.Db4o.CS;
using Db4objects.Db4o.CS.Config;
using Db4objects.Db4o.Messaging;

namespace PrestoDatabaseServer
{
    // Nice article on setting a windows service that can be run as a console app
    // http://stevesmithblog.com/blog/create-a-windows-service-in-net-that-can-also-run-as-console-application/
    // The one gotcha was that I forgot to change the project's output type to Console Application.

    /// <summary>
    /// Presto Database Server Host 
    /// </summary>
    public partial class PrestoDatabaseServerHost : ServiceBase, IMessageRecipient
    {
        private IObjectServer _db4oServer;
        private static PrestoDatabaseServerHost _prestoDatabaseServerHost;

        #region [Constructors]

        /// <summary>
        /// Initializes a new instance of the <see cref="PrestoDatabaseServerHost"/> class.
        /// </summary>
        public PrestoDatabaseServerHost()
        {
            InitializeComponent();
        }

        #endregion

        #region [IMessageRecipient Implementation]

        /// <summary>
        /// Processes the message.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="message">The message.</param>
        public void ProcessMessage(IMessageContext context, object message)
        {
            // Doing nothing is fine. The db4o tutorial will close the server when this method is called by a client.
            // We won't be doing that. We'll be starting/stopping our server via the windows service console.            
        }

        #endregion

        #region [Main]

        private static void Main(string[] args)
        {
            try
            {
                _prestoDatabaseServerHost = new PrestoDatabaseServerHost();

                if (!Environment.UserInteractive)
                {
                    Run(_prestoDatabaseServerHost);
                    return;
                }

                // Run service as console app
                _prestoDatabaseServerHost.OnStart(args);
                Console.WriteLine(PrestoServerResources.ServerStartedConsoleMessage);
                Console.ReadKey();
                _prestoDatabaseServerHost.OnStop();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region [Protected Override Methods]

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager
        /// (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                StartDatabase();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM).
        /// Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                this._db4oServer.Close();
                _prestoDatabaseServerHost.Dispose();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        #endregion

        #region [Private Methods]

        private void StartDatabase()
        {
            IServerConfiguration serverConfiguration = Db4oClientServer.NewServerConfiguration();

            serverConfiguration.Networking.MessageRecipient = this;

            string db4oDatabasePath     = AppDomain.CurrentDomain.BaseDirectory;
            string db4oDatabaseFileName = ConfigurationManager.AppSettings["db4oDatabaseFileName"];            
            int databaseServerPort      = Convert.ToInt32(ConfigurationManager.AppSettings["databaseServerPort"], CultureInfo.InvariantCulture);

            _db4oServer = Db4oClientServer.OpenServer(serverConfiguration, db4oDatabasePath + db4oDatabaseFileName, databaseServerPort);

            string databaseUser     = ConfigurationManager.AppSettings["databaseUser"];
            string databasePassword = ConfigurationManager.AppSettings["databasePassword"];

            _db4oServer.GrantAccess(databaseUser, databasePassword);

            // Note: I was able to get transparent persistence working, but decided against it because of the db4o code
            //       that would have to be littered throughout the domain entities. This is my post about it:
            //       http://stackoverflow.com/questions/8377537/db4o-transparent-persistence-not-working/8380590#8380590
        }

        private static void LogException(Exception ex)
        {
            EventLog.WriteEntry("db4oServer", ex.ToString(), EventLogEntryType.Error);
        }

        #endregion
    }
}