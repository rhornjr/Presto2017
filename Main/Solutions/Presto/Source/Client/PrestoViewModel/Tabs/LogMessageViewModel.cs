using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class LogMessageViewModel : ViewModelBase
    {
        private Collection<LogMessage> _logMessages;

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the installation summary list.
        /// </summary>
        public Collection<LogMessage> LogMessages
        {
            get { return this._logMessages; }

            private set
            {
                this._logMessages = value;
                NotifyPropertyChanged(() => this.LogMessages);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationSummaryViewModel"/> class.
        /// </summary>
        public LogMessageViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadLogMessages();
        }

        private void LoadLogMessages()
        {
            this.LogMessages = new Collection<LogMessage>(LogMessageLogic.GetMostRecentByCreatedTime(50).ToList());
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(_ => Refresh());
        }

        private void Refresh()
        {
            this.LoadLogMessages();

            ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.LogMessagesRefreshed;
        }
    }
}
