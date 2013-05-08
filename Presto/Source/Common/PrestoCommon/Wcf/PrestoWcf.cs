using System;
using System.Configuration;
using System.ServiceModel;
using PrestoCommon.Interfaces;

namespace PrestoCommon.Wcf
{
    /// <summary>
    /// Helper class to encapsulate the details of making a Presto WCF call
    /// </summary>
    public class PrestoWcf : IDisposable
    {
        private WcfChannelFactory<IPrestoService> _channelFactory;

        /// <summary>
        /// Helper property to make Presto WCF calls
        /// </summary>
        /// <example>
        /// using (var prestoWcf = new PrestoWcf())
        /// {
        ///     this.Applications = new ObservableCollection<Application>(prestoWcf.Service.GetAllApplications());
        /// }
        /// </example>
        public IPrestoService Service
        {
            get
            {
                var netTcpBinding = new NetTcpBinding();
                netTcpBinding.MaxReceivedMessageSize = int.MaxValue;

                _channelFactory = new WcfChannelFactory<IPrestoService>(netTcpBinding);
                var endpointAddress = ConfigurationManager.AppSettings["prestoServiceAddress"];

                // The call to CreateChannel() actually returns a proxy that can intercept calls to the
                // service. This is done so that the proxy can retry on communication failures.            
                return _channelFactory.CreateChannel(new EndpointAddress(endpointAddress));
            }
        }

        /// <summary>
        /// Invokes the func on a WCF implementation of IPrestoService
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="func">The action to take against the service</param>
        /// <example>
        /// this.Applications = new ObservableCollection<Application>(PrestoWcf.Invoke(service => service.GetAllApplications()));
        /// </example>
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool calledFromDisposeAndNotFromFinalizer)
        {
            if (calledFromDisposeAndNotFromFinalizer)
            {
                if (_channelFactory != null) { (_channelFactory as IDisposable).Dispose(); }
            }
        }
    }
}
