using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationSummaryViewModel : ViewModelBase
    {
        private Collection<InstallationSummary> _installationSummaryList;
        private InstallationSummary _selectedInstallationSummary;

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the installation summary list.
        /// </summary>
        public Collection<InstallationSummary> InstallationSummaryList
        {
            get { return this._installationSummaryList; }

            private set
            {
                this._installationSummaryList = value;
                NotifyPropertyChanged(() => this.InstallationSummaryList);
            }
        }

        /// <summary>
        /// Gets or sets the selected installation summary.
        /// </summary>
        /// <value>
        /// The selected installation summary.
        /// </value>
        public InstallationSummary SelectedInstallationSummary
        {
            get { return this._selectedInstallationSummary; }

            set
            {
                this._selectedInstallationSummary = value;
                NotifyPropertyChanged(() => this.SelectedInstallationSummary);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationSummaryViewModel"/> class.
        /// </summary>
        public InstallationSummaryViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadInstallationSummaryList();
        }

        private void LoadInstallationSummaryList()
        {
            try
            {
                this.InstallationSummaryList = new Collection<InstallationSummary>(InstallationSummaryLogic.GetMostRecentByStartTime(50).ToList());
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = "Could not load form. Please see log for details.";
            }
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(Refresh);
        }

        private void Refresh()
        {
            this.LoadInstallationSummaryList();

            ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.InstallationListRefreshed;
        }
    }
}
