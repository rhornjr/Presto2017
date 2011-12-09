﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerViewModel : ViewModelBase
    {
        private Collection<ApplicationServer> _applicationServers;
        private ApplicationServer _selectedApplicationServer;
        private Application _selectedApplication;

        /// <summary>
        /// Gets the add application command.
        /// </summary>
        public ICommand AddApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the remove application command.
        /// </summary>
        public ICommand RemoveApplicationCommand { get; private set; }

        /// <summary>
        /// Gets or sets the application servers.
        /// </summary>
        /// <value>
        /// The application servers.
        /// </value>
        public Collection<ApplicationServer> ApplicationServers
        {
            get { return this._applicationServers; }

            private set
            {
                this._applicationServers = value;
                this.NotifyPropertyChanged(() => this.ApplicationServers);
            }
        }

        /// <summary>
        /// Gets or sets the selected application server.
        /// </summary>
        /// <value>
        /// The selected application server.
        /// </value>
        public ApplicationServer SelectedApplicationServer
        {
            get { return this._selectedApplicationServer; }

            set
            {
                this._selectedApplicationServer = value;
                this.NotifyPropertyChanged(() => this.SelectedApplicationServer);
            }
        }

        /// <summary>
        /// Gets or sets the selected application.
        /// </summary>
        /// <value>
        /// The selected application.
        /// </value>
        public Application SelectedApplication
        {
            get { return this._selectedApplication; }

            set
            {
                this._selectedApplication = value;
                this.NotifyPropertyChanged(() => this.SelectedApplication);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServerViewModel"/> class.
        /// </summary>
        public ApplicationServerViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadApplicationServers();
        }

        private void Initialize()
        {
            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication());
            this.RemoveApplicationCommand = new RelayCommand(_ => RemoveApplication());
        }        

        private  void AddApplication()
        {
            ApplicationSelectorViewModel viewModel = new ApplicationSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplicationServer.Applications.Add(viewModel.SelectedApplication);

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }

        private void RemoveApplication()
        {
            this.SelectedApplicationServer.Applications.Remove(this.SelectedApplication);

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }

        private void LoadApplicationServers()
        {
            try
            {
                this.ApplicationServers = new Collection<ApplicationServer>(ApplicationServerLogic.GetAll().ToList());
            }
            catch (SocketException ex)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.DatabaseConnectionFailureMessage;
                LogUtility.LogException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.DatabaseInvalidOperation;
                LogUtility.LogException(ex);
            }
        }
    }
}
