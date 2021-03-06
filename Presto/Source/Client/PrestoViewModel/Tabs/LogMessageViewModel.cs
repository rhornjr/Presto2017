﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
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
            Debug.WriteLine("LogMessageViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadLogMessages();

            Debug.WriteLine("LogMessageViewModel constructor end " + DateTime.Now);
        }

        private void LoadLogMessages()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    this.LogMessages =
                        new Collection<LogMessage>(prestoWcf.Service.GetMostRecentLogMessagesByCreatedTime(50).ToList());
                }
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(Refresh);
        }

        private void Refresh()
        {
            this.LoadLogMessages();

            ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.LogMessagesRefreshed);
        }
    }
}
