using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using ConsoleTestRunner.RavenTestDataClasses;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoServer.Data.RavenDb;

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
                TestAddingXmlNode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Press any key to stop the program.");
            Console.ReadKey();
        }

        private static void TestAddingXmlNode()
        {
            var xmlModify = new TaskXmlModify("test", 1, 1, false, @"C:\Temp\processor.config",
                "DependencyContainerConfiguration/ContainerMapping/add", "Interface", "ISnuh", "ConcreteType", "Snuh");
            xmlModify.AddNode = true;

            xmlModify.Execute(null, null); // Just need to skip the first couple lines of the method because we don't care about resolving variables with this test.
        }

        private static void TestInstallationSummaryData()
        {
            var server = new ApplicationServer();
            server.Id = "ApplicationServers/10";

            var appWithGroup = new ApplicationWithOverrideVariableGroup();
            appWithGroup.Application = new Application();
            appWithGroup.Application.Id = "applications/5";

            appWithGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            appWithGroup.CustomVariableGroups.Add(new CustomVariableGroup());
            appWithGroup.CustomVariableGroupIds = new List<string>();
            appWithGroup.CustomVariableGroupIds.Add("CustomVariableGroups/296");
            appWithGroup.CustomVariableGroupIds.Add("CustomVariableGroups/302");
            appWithGroup.CustomVariableGroupIds.Add("CustomVariableGroups/268");

            var data = new InstallationSummaryData();
            data.SetAsInitialDalInstanceAndCreateSession();

            var installationSummary = data.GetMostRecentByServerAppAndGroup(server, appWithGroup);

            Debug.WriteLine(installationSummary == null);
        }

        private static void TestDataCall()
        {
            var cvg = new CustomVariableGroup();
            cvg.Id = "CustomVariableGroups/197";

            var data = new SandboxData();
            data.SetAsInitialDalInstanceAndCreateSession();
            SandboxData.VerifyGroupNotUsedByInstallationSummary(cvg);
        }

        private static void TestFindingOneAppWithGroupOutOfMany()
        {
            var singleCvg = new CustomVariableGroup() { Id = "884" };
            var extraCvg1 = new CustomVariableGroup() { Id = "1" };
            var extraCvg2 = new CustomVariableGroup() { Id = "2" };
            var extraCvg3 = new CustomVariableGroup() { Id = "3" };
            var extraCvg4 = new CustomVariableGroup() { Id = "4" };

            var app1 = new Application() { Id = "atp" };
            var app2 = new Application() { Id = "fdp" };
            var app3 = new Application() { Id = "mrp" };

            var appWithGroup1 = new ApplicationWithOverrideVariableGroup() { Application = app1 };
            var appWithGroup2 = new ApplicationWithOverrideVariableGroup() { Application = app2 };
            var appWithGroup3 = new ApplicationWithOverrideVariableGroup() { Application = app3 };

            var appWithGroup4 = new ApplicationWithOverrideVariableGroup() { Application = app1 };
            appWithGroup4.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            appWithGroup4.CustomVariableGroups.Add(extraCvg1);

            var appWithGroup5 = new ApplicationWithOverrideVariableGroup() { Application = app2 };
            appWithGroup5.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            appWithGroup5.CustomVariableGroups.Add(extraCvg1);
            appWithGroup5.CustomVariableGroups.Add(extraCvg2);

            var appWithGroup6 = new ApplicationWithOverrideVariableGroup() { Application = app1 };
            appWithGroup6.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            appWithGroup6.CustomVariableGroups.Add(extraCvg3);
            appWithGroup6.CustomVariableGroups.Add(extraCvg4);

            var appWithGroup7 = new ApplicationWithOverrideVariableGroup() { Application = app3 };
            appWithGroup7.CustomVariableGroups = null;

            var appWithGroup8 = new ApplicationWithOverrideVariableGroup() { Application = app1 };
            appWithGroup8.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            appWithGroup8.CustomVariableGroups.Add(singleCvg);
            appWithGroup8.CustomVariableGroups.Add(extraCvg4);

            var appWithGroupList = new List<ApplicationWithOverrideVariableGroup>();
            var appWithGroupToFind = appWithGroup8;

            appWithGroupList.Add(appWithGroup1);
            appWithGroupList.Add(appWithGroup2);
            appWithGroupList.Add(appWithGroup3);
            appWithGroupList.Add(appWithGroup4);
            appWithGroupList.Add(appWithGroup5);
            appWithGroupList.Add(appWithGroup6);
            appWithGroupList.Add(appWithGroup7);
            appWithGroupList.Add(appWithGroup8);

            var appWithGroupMatch = appWithGroupList.FirstOrDefault(groupFromList =>
                    groupFromList.Application.Id == appWithGroupToFind.Application.Id &&
                    groupFromList.CustomVariableGroups != null &&
                    groupFromList.CustomVariableGroups.Count == appWithGroupToFind.CustomVariableGroups.Count &&
                    groupFromList.CustomVariableGroups.Select(x => x.Id).All(appWithGroupToFind.CustomVariableGroups.Select(x => x.Id).Contains));

            // In the final line of the query above, we're selecting all of the IDs of each CVG and making sure the same IDs
            // exist in the appWithGroupToFind.

            if (appWithGroupMatch == null)
            {
                Console.WriteLine("No match found.");
                return;
            }

            Console.WriteLine(appWithGroupMatch.Application.Id);

            if (appWithGroupMatch.CustomVariableGroups != null)
            {
                appWithGroupMatch.CustomVariableGroups.ToList().ForEach(x => Console.WriteLine(x.Id));
            }
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
