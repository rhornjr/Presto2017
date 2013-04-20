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
    public class ApplicationSelectorViewModel : ViewModelBase
    {
        private Collection<Application> _applications;
        private Application _selectedApplication;

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
        public Collection<Application> Applications
        {
            get { return this._applications; }

            private set
            {
                this._applications = value;
                this.NotifyPropertyChanged(() => this.Applications);
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
        /// Initializes a new instance of the <see cref="ApplicationSelectorViewModel"/> class.
        /// </summary>
        public ApplicationSelectorViewModel()
        {
            // ToDo: This constructor should take a list of existing applications that are already associated
            //       with a server so we don't display them.
            Initialize();
        }

        private void Initialize()
        {
            this.AddCommand    = new RelayCommand(Add);
            this.CancelCommand = new RelayCommand(Cancel);

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
                this.Applications = new Collection<Application>(ApplicationLogic.GetAll().OrderBy(x => x.Name).ToList());
            }
            catch (SocketException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseConnectionFailureMessage);
                LogUtility.LogException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseInvalidOperation);
                LogUtility.LogException(ex);
            }
        }        
    }
}
