using System;
using System.Globalization;
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
    public class GlobalSettingViewModel : ViewModelBase
    {
        private GlobalSetting _globalSetting;

        /// <summary>
        /// Gets the refresh command.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        public ICommand SaveCommand { get; private set; }

        /// <summary>
        /// Gets the global setting.
        /// </summary>
        public GlobalSetting GlobalSetting
        {
            get { return this._globalSetting; }

            private set
            {
                this._globalSetting = value;
                NotifyPropertyChanged(() => this.GlobalSetting);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalSettingViewModel"/> class.
        /// </summary>
        public GlobalSettingViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadGlobalSetting();
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(Refresh);
            this.SaveCommand = new RelayCommand(Save);
        }

        private void LoadGlobalSetting()
        {
            try
            {
                this.GlobalSetting = GlobalSettingLogic.GetItem();

                if (this.GlobalSetting == null) { this.GlobalSetting = new GlobalSetting(); }
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = "Could not load form. Please see log for details.";
            }
        }

        private void Refresh()
        {
            this.LoadGlobalSetting();

            ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.GlobalSettingsRefreshed;
        }

        private void Save()
        {
            GlobalSettingLogic.Save(this.GlobalSetting);

            LogMessageLogic.SaveLogMessage(string.Format(CultureInfo.CurrentCulture,
                "Global settings updated. Freeze all installations is now {0}.",
                this.GlobalSetting.FreezeAllInstallations));

            ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.GlobalSettingsSaved;
        }
    }
}
