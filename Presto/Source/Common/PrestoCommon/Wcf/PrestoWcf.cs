using System;
using System.Configuration;
using System.ServiceModel;
using PrestoCommon.Interfaces;

namespace PrestoCommon.Wcf
{
    public static class PrestoWcf
    {
        public static T Invoke<T>(Func<IPrestoService, T> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;

            using (var channelFactory = new WcfChannelFactory<IPrestoService>(netTcpBinding))
            {
                var endpointAddress = ConfigurationManager.AppSettings["prestoServiceAddress"];

                // The call to CreateChannel() actually returns a proxy that can intercept calls to the
                // service. This is done so that the proxy can retry on communication failures.            
                IPrestoService prestoService = channelFactory.CreateChannel(new EndpointAddress(endpointAddress));
                return func(prestoService);
            }
        }
    }
}
