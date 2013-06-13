using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Microsoft.Practices.Unity;
using PrestoCommon.Entities;
using PrestoCommon.Factories.OpenFileDialog;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel;
using PrestoViewModel.Mvvm;
using Xanico.Core;

namespace PrestoDashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(this.AppDispatcherUnhandledException);

            base.OnStartup(e);

            // Use a real (as opposed to mock) open file dialog.
            CommonUtility.Container.RegisterType<IOpenFileDialogService, OpenFileDialogService>();

            MainWindowViewModel.ViewLoader = RegisterViewModelsAndTypes();

            EnsureInitialNecessaryDataExists();

            InitializeSignalR();
        }

        private static void InitializeSignalR()
        {
            var signalrAddress = ConfigurationManager.AppSettings["signalrAddress"];
            var hubConnection = new HubConnection(signalrAddress);
            var prestoHubProxy = hubConnection.CreateHubProxy("PrestoHub");
            prestoHubProxy.On<string>("OnSignalRMessage", OnSignalRMessage);
            hubConnection.Start();
        }

        private static void OnSignalRMessage(string data)
        {
            MessageBox.Show(data);
        }

        private static void EnsureInitialNecessaryDataExists()
        {
            PossiblyAddInstallationEnvironments();
            PossiblyAddGlobalSetting();
        }

        private static void PossiblyAddGlobalSetting()
        {
            GlobalSetting globalSetting;
            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                globalSetting = prestoWcf.Service.GetGlobalSettingItem();
            }

            if (globalSetting != null) { return; }

            AddGlobalSetting();
        }

        private static void AddGlobalSetting()
        {
            GlobalSetting globalSetting = new GlobalSetting();
            globalSetting.FreezeAllInstallations = false;

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveGlobalSetting(globalSetting);

                string logMessage = string.Format(CultureInfo.CurrentCulture,
                    "Global setting didn't exist, so it was created with Freeze All Installations set to [{0}].",
                    globalSetting.FreezeAllInstallations);
            
                prestoWcf.Service.SaveLogMessage(logMessage);
            }
        }

        private static void PossiblyAddInstallationEnvironments()
        {
            List<InstallationEnvironment> environments;
            using (var prestoWcf = new PrestoWcf<IInstallationEnvironmentService>())
            {
                environments = prestoWcf.Service.GetAllInstallationEnvironments().ToList();
            }

            if (environments.Count > 0) { return; }

            AddInstallationEnvironment("Development", 1);
            AddInstallationEnvironment("QA", 2);
            AddInstallationEnvironment("Staging", 3);
            AddInstallationEnvironment("Production", 4);
        }

        private static void AddInstallationEnvironment(string name, int logicalOrder)
        {
            InstallationEnvironment env = new InstallationEnvironment();
            env.Name = name;
            env.LogicalOrder = logicalOrder;

            using (var prestoWcf = new PrestoWcf<IInstallationEnvironmentService>())
            {
                prestoWcf.Service.Save(env);
            }

            string logMessage = string.Format(CultureInfo.CurrentCulture,
                "Created environment [{0}] with logical order of [{1}]",
                env.Name,
                env.LogicalOrder);

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveLogMessage(logMessage);
            }
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.LogException(e.Exception);

            string message = string.Format(CultureInfo.CurrentCulture, PrestoDashboardResource.ErrorMessage, e.Exception.Message);

            MessageBox.Show(message, PrestoDashboardResource.ErrorCaption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            
            e.Handled = true;
            
            System.Windows.Application.Current.Shutdown();
        }

        private static ViewLoader RegisterViewModelsAndTypes()
        {
            ViewLoader viewLoader = new ViewLoader();

            Assembly assembly = Assembly.GetExecutingAssembly();

            List<Type> types = new List<Type>(assembly.GetTypes());

            Attribute[] attributes;

            foreach (Type type in types)
            {
                attributes = Attribute.GetCustomAttributes(type);

                foreach (ViewModelAttribute attribute in attributes.Where(attr => attr is ViewModelAttribute))
                {
                    viewLoader.Register(attribute.ViewModelType, type);
                }
            }

            return viewLoader;
        }        
    }
}
