using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Data.RavenDb;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
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
        private List<InstallationSummaryDto> _installationSummaryDtos;
        private InstallationSummary _selectedInstallationSummary;
        private InstallationSummaryDto _selectedInstallationSummaryDto;
        private Collection<ITimeZoneHelper> _timeZoneHelpers;
        private ITimeZoneHelper _selectedTimeZoneHelper;

        public ICommand RefreshCommand { get; private set; }

        public Collection<InstallationSummary> InstallationSummaryList
        {
            get { return this._installationSummaryList; }

            private set
            {
                this._installationSummaryList = value;
                NotifyPropertyChanged(() => this.InstallationSummaryList);
            }
        }

        public List<InstallationSummaryDto> InstallationSummaryDtos
        {
            get { return this._installationSummaryDtos; }

            set
            {
                this._installationSummaryDtos = value;
                NotifyPropertyChanged(() => this.InstallationSummaryDtos);
            }
        }

        public Collection<ITimeZoneHelper> TimeZoneHelpers
        {
            get { return this._timeZoneHelpers; }

            set
            {
                this._timeZoneHelpers = value;
                NotifyPropertyChanged(() => this.TimeZoneHelpers);
            }
        }

        public ITimeZoneHelper SelectedTimeZoneHelper
        {
            get { return this._selectedTimeZoneHelper; }

            set
            {
                if (this._selectedTimeZoneHelper != value)
                {
                    this._selectedTimeZoneHelper = value;
                    NotifyPropertyChanged(() => this.SelectedTimeZoneHelper);
                    SetInstallationSummaryDtos();
                }
            }
        }

        public InstallationSummary SelectedInstallationSummary
        {
            get { return this._selectedInstallationSummary; }

            set
            {
                this._selectedInstallationSummary = value;
                NotifyPropertyChanged(() => this.SelectedInstallationSummary);
            }
        }

        public InstallationSummaryDto SelectedInstallationSummaryDto
        {
            get { return this._selectedInstallationSummaryDto; }

            set
            {
                if (value != null)
                {
                    this._selectedInstallationSummaryDto = value;
                    NotifyPropertyChanged(() => this.SelectedInstallationSummaryDto);

                    // Set the real installation summary based on the ID of the DTO
                    this.SelectedInstallationSummary = this.InstallationSummaryList.Where(x => x.Id == value.Id).First();
                }
            }
        }

        public InstallationSummaryViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadTimeZones();
            LoadInstallationSummaryList();
            SubscribeToDatabaseChangeEvents();
        }

        private void SubscribeToDatabaseChangeEvents()
        {
            // When there is a new installation summary, automatically refresh the list.
            DataAccessLayerBase.NewInstallationSummaryAddedToDb += OnDatabaseItemAdded;
        }

        private void OnDatabaseItemAdded(object sender, EventArgs<string> e)
        {
            Refresh();
        }

        private void LoadInstallationSummaryList()
        {
            try
            {
                this.InstallationSummaryList = new Collection<InstallationSummary>(InstallationSummaryLogic.GetMostRecentByStartTime(50).ToList());
                SetInstallationSummaryDtos();
                this.SelectedInstallationSummary = null;
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }

        private void SetInstallationSummaryDtos()
        {
            if (this.InstallationSummaryList == null || this.InstallationSummaryList.Count < 1) { return; }

            List<InstallationSummaryDto> installationSummaryDtos = new List<InstallationSummaryDto>();

            foreach (InstallationSummary installationSummary in this.InstallationSummaryList)
            {
                InstallationSummaryDto dto = new InstallationSummaryDto();
                dto.ApplicationName        = installationSummary.ApplicationWithOverrideVariableGroup.ToString();
                dto.Id                     = installationSummary.Id;                
                dto.Result                 = installationSummary.InstallationResult.ToString();
                dto.ServerName             = installationSummary.ApplicationServer.Name;

                this.SelectedTimeZoneHelper.SetStartAndEndTimes(installationSummary, dto);

                installationSummaryDtos.Add(dto);
            }

            this.InstallationSummaryDtos = installationSummaryDtos.OrderByDescending(x => x.InstallationStart).ToList();
        }

        private void LoadTimeZones()
        {
            Collection<ITimeZoneHelper> timeZoneHelpers = new Collection<ITimeZoneHelper>();

            timeZoneHelpers.Add(new TimeZoneHelperThisComputer());
            timeZoneHelpers.Add(new TimeZoneHelperInstallingServer());            
            timeZoneHelpers.Add(new TimeZoneHelperUtc());

            this.TimeZoneHelpers = timeZoneHelpers;

            this.SelectedTimeZoneHelper = this.TimeZoneHelpers[0];  // The first one is our default.
        }

        private void Initialize()
        {
            this.RefreshCommand = new RelayCommand(Refresh);
        }

        private void Refresh()
        {
            this.LoadInstallationSummaryList();

            ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.InstallationListRefreshed);
        }
    }
}
