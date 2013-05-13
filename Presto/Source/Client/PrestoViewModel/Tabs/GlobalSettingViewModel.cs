using System;
using System.Globalization;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using Xanico.Core;

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
                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    this.GlobalSetting = prestoWcf.Service.GetGlobalSettingItem();
                }

                if (this.GlobalSetting == null) { this.GlobalSetting = new GlobalSetting(); }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }

        private void Refresh()
        {
            this.LoadGlobalSetting();

            ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.GlobalSettingsRefreshed);
        }

        private void Save()
        {
            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                this.GlobalSetting = prestoWcf.Service.SaveGlobalSetting(this.GlobalSetting);
            }

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveLogMessage(string.Format(CultureInfo.CurrentCulture,
                "Global settings updated. Freeze all installations is now {0}.",
                this.GlobalSetting.FreezeAllInstallations));
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.GlobalSettingsSaved);
        }
    }
}
