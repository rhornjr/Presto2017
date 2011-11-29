using System;
using System.ServiceProcess;
using Db4objects.Db4o.Messaging;

namespace PrestoDatabaseServer
{
    public partial class PrestoDatabaseServer : ServiceBase, IMessageRecipient
    {
        public PrestoDatabaseServer()
        {
            InitializeComponent();
        }

        private static void Main(string[] args)
        { }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }

        public void ProcessMessage(IMessageContext context, object message)
        {
            throw new NotImplementedException();
        }
    }
}
