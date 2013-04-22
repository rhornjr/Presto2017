using System;
using System.ServiceModel;

namespace ConsoleTestRunner.WcfHelpers
{
    public class WcfChannelFactory<T> : ChannelFactory<T> where T : class
    {
        public override T CreateChannel(EndpointAddress address, Uri via)
        {
            this.Endpoint.Address = address;
            var proxy = new WcfClientProxy<T>(this);
            return proxy.GetTransparentProxy() as T;
        }
    }
}
