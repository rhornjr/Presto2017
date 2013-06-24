using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using Xanico.Core;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerSelectorViewModel : ViewModelBase
    {
        private IEnumerable<ApplicationServer> _servers;
        private ApplicationServer _selectedServers;
        private List<InstallationEnvironment> _installationEnvironments;
        private InstallationEnvironment _selectedInstallationEnvironment;

        /// <summary>
        /// Gets the add command.
        /// </summary>
        public ICommand AddCommand { get; private set; }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<InstallationEnvironment> InstallationEnvironments
        {
            get
            {
                if (this._installationEnvironments == null)
                {
                    using (var prestoWcf = new PrestoWcf<IInstallationEnvironmentService>())
                    {
                        this._installationEnvironments =
                            prestoWcf.Service.GetAllInstallationEnvironments().OrderBy(x => x.LogicalOrder).ToList();
                    }
                }
                return this._installationEnvironments;
            }
        }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public IEnumerable<ApplicationServer> Servers
        {
            get
            {
                if (this.InstallationEnvironments == null || this._servers == null) { return null; }
                return this._servers.Where(x => x.InstallationEnvironment.Id == this.SelectedInstallationEnvironment.Id);
            }

            private set
            {
                this._servers = value;
                this.NotifyPropertyChanged(() => this.Servers);
            }
        }

        public InstallationEnvironment SelectedInstallationEnvironment
        {
            get
            {
                if (this._selectedInstallationEnvironment == null)
                {
                    this._selectedInstallationEnvironment = this.InstallationEnvironments.First();
                }
                return this._selectedInstallationEnvironment;
            }

            set
            {
                if (this._selectedInstallationEnvironment == value) { return; }
                this._selectedInstallationEnvironment = value;
                this.NotifyPropertyChanged(() => this.Servers);
            }
        }

        /// <summary>
        /// Gets or sets the selected application.
        /// </summary>
        /// <value>
        /// The selected application.
        /// </value>
        public ApplicationServer SelectedServer
        {
            get { return this._selectedServers; }

            set
            {
                this._selectedServers = value;
                this.NotifyPropertyChanged(() => this.SelectedServer);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSelectorViewModel"/> class.
        /// </summary>
        public ApplicationServerSelectorViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.AddCommand    = new RelayCommand(Add, CanAdd);
            this.CancelCommand = new RelayCommand(Cancel);

            LoadServers();
        }

        private bool CanAdd()
        {
            if (this.SelectedServer == null) { return false; }
            return true;
        }

        private void Add()
        {
            // Since we're dealing with the slim version of servers, get the fully-hydrated version of the
            // selected server.
            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                this.SelectedServer = prestoWcf.Service.GetServerById(this.SelectedServer.Id);
            }

            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void LoadServers()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    this.Servers = new Collection<ApplicationServer>(
                        prestoWcf.Service.GetAllServersSlim().OrderBy(x => x.Name).ToList());
                }
            }
            catch (SocketException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseConnectionFailureMessage);
                CommonUtility.ProcessException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseInvalidOperation);
                CommonUtility.ProcessException(ex);
            }
        }
    }
}
