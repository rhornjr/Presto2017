using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using PrestoViewModel;
using PrestoViewModel.Mvvm;

namespace PrestoDashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(this.AppDispatcherUnhandledException);
            //DispatcherHelper.Initialize();

            base.OnStartup(e);

            MainWindowViewModel.ViewLoader = RegisterViewModelsAndTypes();

            //MainTabControlViewModel.MainViewLoader = Utility.CreateViewLoaderWithViewAndViewModelAssociations<MainTabControlView, MainTabControlViewModel>();
        }
        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // ToDo: Create ability to log.
            //LogUtility.LogError(e.Exception);
            MessageBox.Show(string.Format(PrestoDashboardResource.ErrorMessage, e.Exception.Message), PrestoDashboardResource.ErrorCaption,
                MessageBoxButton.OK, MessageBoxImage.Exclamation);
            e.Handled = true;
        }

        private ViewLoader RegisterViewModelsAndTypes()
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
