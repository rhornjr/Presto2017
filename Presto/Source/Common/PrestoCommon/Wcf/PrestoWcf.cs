﻿using System;
using System.Configuration;
using System.ServiceModel;

namespace PrestoCommon.Wcf
{
    /// <summary>
    /// Helper class to encapsulate the details of making a Presto WCF call
    /// </summary>
    public class PrestoWcf<TService> : IDisposable where TService : class
    {
        private WcfChannelFactory<TService> _channelFactory;
        private string _prestoServiceAddress = string.Empty;

        /// <summary>
        /// Helper property to make Presto WCF calls
        /// </summary>
        /// <example>
        /// using (var prestoWcf = new PrestoWcf())
        /// {
        ///     this.Applications = new ObservableCollection<Application>(prestoWcf.Service.GetAllApplications());
        /// }
        /// </example>
        public TService Service
        {
            get
            {
                var netTcpBinding = new NetTcpBinding();
                netTcpBinding.MaxReceivedMessageSize = int.MaxValue;

                _channelFactory = new WcfChannelFactory<TService>(netTcpBinding);

                // The call to CreateChannel() actually returns a proxy that can intercept calls to the
                // service. This is done so that the proxy can retry on communication failures.            
                return _channelFactory.CreateChannel(GetEndpointAddress());
            }
        }

        public PrestoWcf()
        {
            this._prestoServiceAddress = ConfigurationManager.AppSettings["prestoServiceAddress"];
        }

        public PrestoWcf(string prestoServiceAddress)
        {
            this._prestoServiceAddress = prestoServiceAddress;
        }

        /// <summary>
        /// Invokes the func on a WCF implementation of a specific IPrestoService (T)
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="func">The action to take against the service</param>
        /// <example>
        /// this.Applications = new ObservableCollection<Application>(PrestoWcf.Invoke(service => service.GetAllApplications()));
        /// </example>
        public TReturn Invoke<TReturn>(Func<TService, TReturn> func)
        {
            if (func == null) { throw new ArgumentNullException("func"); }

            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.MaxReceivedMessageSize = int.MaxValue;

            using (var channelFactory = new WcfChannelFactory<TService>(netTcpBinding))
            {
                // The call to CreateChannel() actually returns a proxy that can intercept calls to the
                // service. This is done so that the proxy can retry on communication failures.            
                TService prestoService = channelFactory.CreateChannel(GetEndpointAddress());
                return func(prestoService);
            }
        }

        private EndpointAddress GetEndpointAddress()
        {
            // We need to use a dummy SPN here because we were getting "target principal name is incorrect" errors.
            // http://inaspiralarray.blogspot.com/2013/05/wcf-security-issue-target-principal.html
            var uri              = new Uri(this._prestoServiceAddress);
            var endpointIdentity = EndpointIdentity.CreateSpnIdentity(string.Empty);

            return new EndpointAddress(uri, endpointIdentity);
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
                if (_channelFactory != null) { _channelFactory.Close(); }
            }
        }
    }
}
