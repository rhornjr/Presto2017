﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using Xanico.Core;
using Xanico.Core.Security;

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
            Debug.WriteLine("GlobalSettingViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadGlobalSetting();

            Debug.WriteLine("GlobalSettingViewModel constructor end " + DateTime.Now);
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

            Logger.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "[{0}] selected the option to update the self-updaters on all servers.", IdentityHelper.UserName));

            ViewModelUtility.MainWindowViewModel.AddUserMessage("Updating self-updater on all servers...");

            // ToDo: Do this on the SERVER side. The user can close the Dashboard and cause this to stop.

            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var prestoWcf = new PrestoWcf<IServerService>())
                    {
                        var allServers = prestoWcf.Service.GetAllServers(false);

                        foreach (var server in allServers)
                        {
                            try
                            {
                                prestoWcf.Service.InstallPrestoSelfUpdater(server, null);
                            }
                            catch (Exception ex)
                            {
                                // Just log and keep trying to process the rest of the servers.
                                CommonUtility.ProcessException(ex);
                            }
                        }
                    }

                    Logger.LogInformation("Done updating self-updater on all servers.");
                }
                catch (Exception ex)
                {
                    // Just log and keep trying to process the rest of the servers.
                    CommonUtility.ProcessException(ex);
                }
            });
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
                CommonUtility.ProcessException(ex);
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
