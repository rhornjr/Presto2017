using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerSelectorViewModel : ViewModelBase
    {
        private Collection<ApplicationServer> _servers;
        private ApplicationServer _selectedServers;

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

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public Collection<ApplicationServer> Servers
        {
            get { return this._servers; }

            private set
            {
                this._servers = value;
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
            this.AddCommand    = new RelayCommand(_ => Add());
            this.CancelCommand = new RelayCommand(_ => Cancel());

            LoadApplications();
        }

        private void Add()
        {
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void LoadApplications()
        {
            try
            {
                this.Servers = new Collection<ApplicationServer>(ApplicationServerLogic.GetAll().OrderBy(x => x.Name).ToList());
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
