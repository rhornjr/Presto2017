using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using PrestoCommon.Interfaces;
using PrestoCommon.Entities;
using PrestoCommon.Wcf;
using System.Diagnostics;

namespace ConsoleTestRunner
{
    class Program
    {
        // Need to find a good way to encapsulate WCF retries. I have this question pending:
        // http://stackoverflow.com/q/16151361/279516

        // Me: Should I keep the channel alive and reuse it for every call? My guess is no.
        // Jim: My experience is that recreating the channel is not an expensive operation and leaving
        //      it open can cause side effects or channel failures or memory leaks on the server. So I
        //      would open the channel, use it, then close it. You can cache the channel factory though.

        // Me: I was going to create a client-side class to deal with the calls. That way I can deal with
        //     retrying the calls in one spot. Is this basically what your resilient proxy does?
        // Jim: The resilient proxy is a low level interception of channel calls. If Communication
        //      exception occurs (not a FaultException) it will attempt a retry of the call. Recent
        //      version will actually trigger a rediscovery of services if the appropriate object hierarchy
        //      is in place. But basic premise is that it retries the WCF service call on
        //      CommunicationException exceptions specifically handling FaultException separately
        //      (FaultException’s are also CommunicationExceptions so you have to handle them specifically).

        // Jim: ResilientProxy uses RealProxy which is stock .net stuff. Using RealProxy, you can create an
        //      actual proxy at runtime using your provided interface. From calling code it is as if they
        //      are using and IFoo channel, but in fact it is a IFoo to the proxy and then you get a chance
        //      to intercept the calls to any method, property, constructor, etc…

        static void Main(string[] args)
        {
            try
            {
                TestGetServers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to stop the program.");
            Console.ReadKey();
        }

        private static void TestGetOneServer()
        {
            var stopwatch = new Stopwatch();

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                stopwatch.Start();
                var server = prestoWcf.Service.GetServerById("ApplicationServers/481");
                stopwatch.Stop();
                Console.WriteLine("GetServerById() took {0} milliseconds.", stopwatch.ElapsedMilliseconds);

                Debug.WriteLine(server.Name);
            }
        }

        private static void TestGetServers()
        {
            var stopwatch = new Stopwatch();

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                stopwatch.Start();
                prestoWcf.Service.GetAllServers(true);
                stopwatch.Stop();
                Console.WriteLine("GetAllServers() took {0} milliseconds.", stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
                stopwatch.Start();
                prestoWcf.Service.GetAllServersSlim();
                stopwatch.Stop();
                Console.WriteLine("GetAllServersSlim() took {0} milliseconds.", stopwatch.ElapsedMilliseconds);
            }
        }

        private static void SomeTesting()
        {
            var channelFactory = new WcfChannelFactory<IApplicationService>(new NetTcpBinding());
            var endpointAddress = ConfigurationManager.AppSettings["prestoServiceAddress"];

            // The call to CreateChannel() actually returns a proxy that can intercept calls to the
            // service. This is done so that the proxy can retry on communication failures.            
            IApplicationService appService = channelFactory.CreateChannel(new EndpointAddress(endpointAddress));

            Console.WriteLine("Enter some information to echo to the Presto service:");
            string message = Console.ReadLine();

            var channelFactoryBase = new WcfChannelFactory<IBaseService>(new NetTcpBinding());
            IBaseService baseService = channelFactoryBase.CreateChannel(new EndpointAddress(endpointAddress));
            string returnMessage = baseService.Echo(message);

            Console.WriteLine("Presto responds: {0}", returnMessage);

            IEnumerable<Application> apps = appService.GetAllApplications(true);

            foreach (var app in apps)
            {
                Console.WriteLine(app.Name);
            }
        }
    }
}
