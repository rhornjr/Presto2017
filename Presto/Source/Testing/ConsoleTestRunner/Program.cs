﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;

namespace ConsoleTestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding netTcpBinding = new NetTcpBinding();

            EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:8087/PrestoWcfService");

            var channelFactory = new ChannelFactory<IPrestoService>(netTcpBinding, endpointAddress);

            IPrestoService prestoService = channelFactory.CreateChannel();

            Console.WriteLine("Enter some information to echo to the Presto service:");
            string message = Console.ReadLine();

            string returnMessage = prestoService.Echo(message);

            Console.WriteLine("Presto responds: {0}", returnMessage);

            List<Application> apps = prestoService.GetAllApplications();

            foreach (var app in apps)
            {
                Console.WriteLine(app.Name);
            }

            Console.WriteLine("Press any key to stop the program.");
            Console.ReadKey();
        }
    }
}
