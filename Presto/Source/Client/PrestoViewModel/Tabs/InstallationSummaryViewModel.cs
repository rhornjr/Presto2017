using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.AspNet.SignalR.Client.Hubs;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.EntityHelperClasses.TimeZoneHelpers;
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
            Debug.WriteLine("InstallationSummaryViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadTimeZones();
            LoadInstallationSummaryList();
            InitializeSignalR();

            Debug.WriteLine("InstallationSummaryViewModel constructor end " + DateTime.Now);
        }

        private async Task InitializeSignalR()
        {
            await Task.Run(() =>
            {
                var hubConnection = new HubConnection(CommonUtility.SignalRAddress);
                var prestoHubProxy = hubConnection.CreateHubProxy("PrestoHub");
                prestoHubProxy.On("OnInstallationSummaryAdded", OnInstallationSummaryAdded);
                hubConnection.Start();
            });
        }

        private void OnInstallationSummaryAdded()
        {
            Refresh();
        }

        private async Task LoadInstallationSummaryList()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var prestoWcf = new PrestoWcf<IInstallationSummaryService>())
                    {
                        this.InstallationSummaryList = new Collection<InstallationSummary>(
                            prestoWcf.Service.GetMostRecentByStartTime(50).ToList());
                    }
                    SetInstallationSummaryDtos();
                    this.SelectedInstallationSummary = null;
                });
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
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

                try
                {
                    dto.ApplicationName = installationSummary.ApplicationWithOverrideVariableGroup.ToString();
                    dto.Id              = installationSummary.Id;
                    dto.Result          = installationSummary.InstallationResult.ToString();
                    dto.ServerName      = installationSummary.ApplicationServer == null ? "(unavailable)" : installationSummary.ApplicationServer.Name;
                }
                catch (NullReferenceException ex)
                {
                    // This is here because we sometimes get this exception. Let's see if we can find out why...
                    // Log everything we can about this installation summary.
                    SendEmailWithInstallationSummaryProperties(installationSummary);
                    CommonUtility.ProcessException(ex);
                    continue;  // Skip this installation summary; something is wrong with it.
                }

                this.SelectedTimeZoneHelper.SetStartAndEndTimes(installationSummary, dto);

                installationSummaryDtos.Add(dto);
            }

            this.InstallationSummaryDtos = installationSummaryDtos.OrderByDescending(x => x.InstallationStart).ToList();
        }

        private static void SendEmailWithInstallationSummaryProperties(InstallationSummary installationSummary)
        {
            string subject = "Presto Warning - Invalid Installation Summary";

            string appName = installationSummary.ApplicationWithOverrideVariableGroup == null
                ? "app with override group is null" : installationSummary.ApplicationWithOverrideVariableGroup.ToString();

            string summaryId  = installationSummary.Id;
            string result     = installationSummary.InstallationResult.ToString();
            string serverName = installationSummary.ApplicationServer == null ? "server is null" : installationSummary.ApplicationServer.Name;

            string message = string.Format(CultureInfo.CurrentCulture,
                "An attempt was made to display an installation summary in the UI, however the installation summary was invalid." +
                Environment.NewLine + Environment.NewLine +
                "App name: {0}" + Environment.NewLine +
                "Installation summary ID: {1}"+ Environment.NewLine +
                "Result: {2}"+ Environment.NewLine +
                "Server name: {3}",
                appName,
                summaryId,
                result,
                serverName);

            CommonUtility.SendEmail(subject, message);
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
