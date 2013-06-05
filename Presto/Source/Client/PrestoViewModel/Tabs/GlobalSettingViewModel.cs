using System;
using System.Globalization;
using System.ServiceModel;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using Xanico.Core;

namespace PrestoViewModel.Tabs
{
    public class GlobalSettingViewModel : ViewModelBase
    {
        private GlobalSetting _globalSetting;

        public ICommand RefreshCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand UpdateSelfUpdaterCommand { get; private set; }

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
            this.RefreshCommand           = new RelayCommand(Refresh);
            this.SaveCommand              = new RelayCommand(Save);
            this.UpdateSelfUpdaterCommand = new RelayCommand(UpdateSelfUpdater);
        }

        private void UpdateSelfUpdater()
        {
            if (!UserChoosesYes("Update the self-updating service host on *every* app server? Be sure you want to do this " +
                "because this is a significant and irreversible action.")) { return; }

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                var allServers = prestoWcf.Service.GetAllServers();

                foreach (var server in allServers)
                {
                    if (server.Name != "FS-6103") { continue; }
                    prestoWcf.Service.InstallPrestoSelfUpdater(server);
                }
            }
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
            try
            {
                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    this.GlobalSetting = prestoWcf.Service.SaveGlobalSetting(this.GlobalSetting);
                }
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);

                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);

                return;
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
