﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerViewModel : ViewModelBase
    {
        private Collection<ApplicationServer> _applicationServers;
        private ApplicationServer _selectedApplicationServer;

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
        /// Initializes a new instance of the <see cref="ApplicationServerViewModel"/> class.
        /// </summary>
        public ApplicationServerViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            LoadApplicationServers();
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