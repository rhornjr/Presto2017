using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using PrestoCommon.Interfaces;

namespace ConsoleTestRunner.WcfHelpers
{
    public class WcfClientProxy<T> : RealProxy where T : class 
    {
        public WcfClientProxy(WcfChannelFactory<T> channelFactory) : base(typeof(T))
        {

        }

        public override IMessage Invoke(IMessage msg)
        {
            IPrestoService prestoService = RetrievePrestoService();

            var methodCall = msg as IMethodCallMessage;
            var methodBase = methodCall.MethodBase;

            var result = methodBase.Invoke(prestoService, methodCall.Args);

            return new ReturnMessage(
                      result, // Operation result
                      null, // Out arguments
                      0, // Out arguments count
                      methodCall.LogicalCallContext, // Call context
                      methodCall); // Original message
        }

        public static IPrestoService RetrievePrestoService()
        {
            NetTcpBinding netTcpBinding = new NetTcpBinding();

            EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:8087/PrestoWcfService");

            var channelFactory = new ChannelFactory<IPrestoService>(netTcpBinding, endpointAddress);

            IPrestoService prestoService = channelFactory.CreateChannel();
            return prestoService;
        }
    }
}
