using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallationSummaryViewModel : ViewModelBase
    {
        private Collection<InstallationSummary> _installationSummaryList;

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
            this.InstallationSummaryList = new Collection<InstallationSummary>(InstallationSummaryLogic.GetMostRecentByStartTime(50).ToList());
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(_ => Refresh());
        }

        private void Refresh()
        {
            this.LoadInstallationSummaryList();
        }
    }
}
