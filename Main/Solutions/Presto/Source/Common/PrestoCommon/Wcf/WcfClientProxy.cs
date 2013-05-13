using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using Xanico.Core;

namespace PrestoCommon.Wcf
{
    public class WcfClientProxy<T> : RealProxy where T : class
    {
        private T _wcfService;
        private WcfChannelFactory<T> _channelFactory;

        public WcfClientProxy(WcfChannelFactory<T> channelFactory)
            : base(typeof(T))
        {
            this._channelFactory = channelFactory;
        }

        public override IMessage Invoke(IMessage msg)
        {
            // When a service method gets called, we intercept it here and call it below with methodBase.Invoke().

            var methodCall = msg as IMethodCallMessage;
            var methodBase = methodCall.MethodBase;
            List<Exception> exceptions = new List<Exception>();

            // We can't call CreateChannel() because that creates an instance of this class,
            // and we'd end up with a stack overflow. So, call CreateBaseChannel() to get the
            // actual service.
            this._wcfService = this._channelFactory.CreateBaseChannel();

            // ToDo: Make the number of retries configurable.
            const int numberOfTimesToTryServiceCall = 5;
            for (int i = 1; i <= numberOfTimesToTryServiceCall; i++)
            {
                try
                {
                    var result = methodBase.Invoke(_wcfService, methodCall.Args);

                    return new ReturnMessage(
                          result, // Operation result
                          null, // Out arguments
                          0, // Out arguments count
                          methodCall.LogicalCallContext, // Call context
                          methodCall); // Original message
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is FaultException) { throw ex.InnerException; }

                    exceptions.Add(ex);
                    Logger.LogException(ex);
                    this.CloseOrAbortService();
                    this.CreateService();
                }
                catch (FaultException)
                {
                    // Need to specifically catch and rethrow FaultExceptions to bypass the CommunicationException catch.
                    // This is needed to distinguish between Faults and underlying communication exceptions.
                    throw;
                }
                catch (CommunicationException ex)
                {
                    exceptions.Add(ex);
                    Logger.LogException(ex);
                    this.CloseOrAbortService();
                    this.CreateService();
                }
            }

            // If we get through all five attempts and still haven't returned successfully (or thrown), then throw.
            throw new AggregateException(String.Format(CultureInfo.InvariantCulture,
                "Unable to invoke method call: {0}", methodCall.MethodName), exceptions);
        }

        private void CreateService()
        {
            this._wcfService = this._channelFactory.CreateBaseChannel();
        }

        private void CloseOrAbortService()
        {
            if (this._wcfService == null) { return; }

            ICommunicationObject wcfService = this._wcfService as ICommunicationObject;

            try
            {
                if (wcfService != null)
                {
                    if (wcfService.State != CommunicationState.Faulted)
                    {
                        wcfService.Close();
                    }
                    else
                    {
                        wcfService.Abort();
                    }
                }
            }
            catch (CommunicationException)
            {
                if (wcfService != null) wcfService.Abort();
            }
            catch (TimeoutException)
            {
                if (wcfService != null) wcfService.Abort();
            }
            catch
            {
                if (wcfService != null) wcfService.Abort();
            }
            finally
            {
                this._wcfService = null;
            }
        }
    }
}
