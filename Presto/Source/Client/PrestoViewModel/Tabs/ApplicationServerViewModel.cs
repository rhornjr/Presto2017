﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerViewModel : ViewModelBase
    {
        private List<DeploymentEnvironment> _deploymentEnvironments;
        private ObservableCollection<ApplicationServer> _applicationServers;
        private ApplicationServer _selectedApplicationServer;
        private ApplicationWithOverrideVariableGroup _selectedApplicationWithOverrideGroup;

        /// <summary>
        /// Gets the add server command.
        /// </summary>
        public ICommand AddServerCommand { get; private set; }

        /// <summary>
        /// Gets the delete server command.
        /// </summary>
        public ICommand DeleteServerCommand { get; private set; }

        /// <summary>
        /// Gets the save server command.
        /// </summary>
        public ICommand SaveServerCommand { get; private set; }

        /// <summary>
        /// Gets the add application command.
        /// </summary>
        public ICommand AddApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the edit application command.
        /// </summary>
        public ICommand EditApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the remove application command.
        /// </summary>
        public ICommand RemoveApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the force application command.
        /// </summary>
        public ICommand ForceApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the add variable group command.
        /// </summary>
        public ICommand AddVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the remove variable group command.
        /// </summary>
        public ICommand RemoveVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the deployment environments.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<DeploymentEnvironment> DeploymentEnvironments
        {
            get
            {
                if (this._deploymentEnvironments == null)
                {
                    this._deploymentEnvironments = Enum.GetValues(typeof(DeploymentEnvironment)).Cast<DeploymentEnvironment>().ToList();
                }
                return this._deploymentEnvironments;
            }
        }

        /// <summary>
        /// Gets or sets the application servers.
        /// </summary>
        /// <value>
        /// The application servers.
        /// </value>
        public ObservableCollection<ApplicationServer> ApplicationServers
        {
            get { return this._applicationServers; }

            private set
            {
                this._applicationServers = value;
                this.NotifyPropertyChanged(() => this.ApplicationServers);
            }
        }

        /// <summary>
        /// Gets or sets the selected application server.
        /// </summary>
        /// <value>
        /// The selected application server.
        /// </value>
        public ApplicationServer SelectedApplicationServer
        {
            get { return this._selectedApplicationServer; }

            set
            {
                this._selectedApplicationServer = value;
                this.NotifyPropertyChanged(() => this.SelectedApplicationServer);
            }
        }

        /// <summary>
        /// Gets or sets the selected custom variable group.
        /// </summary>
        /// <value>
        /// The selected custom variable group.
        /// </value>
        public CustomVariableGroup SelectedCustomVariableGroup { get; set; }

        /// <summary>
        /// Gets or sets the selected application.
        /// </summary>
        /// <value>
        /// The selected application.
        /// </value>
        public ApplicationWithOverrideVariableGroup SelectedApplicationWithOverrideGroup
        {
            get { return this._selectedApplicationWithOverrideGroup; }

            set
            {
                this._selectedApplicationWithOverrideGroup = value;
                this.NotifyPropertyChanged(() => this.SelectedApplicationWithOverrideGroup);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServerViewModel"/> class.
        /// </summary>
        public ApplicationServerViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadApplicationServers();
        }

        private void Initialize()
        {
            this.AddServerCommand    = new RelayCommand(_ => AddServer());
            this.DeleteServerCommand = new RelayCommand(_ => DeleteServer(), _ => AppServerIsSelected());
            this.SaveServerCommand   = new RelayCommand(_ => SaveServer(), _ => AppServerIsSelected());

            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication());
            this.EditApplicationCommand   = new RelayCommand(_ => EditApplication(), _ => ApplicationIsSelected());
            this.RemoveApplicationCommand = new RelayCommand(_ => RemoveApplication(), _ => ApplicationIsSelected());
            this.ForceApplicationCommand  = new RelayCommand(_ => ForceApplication(), _ => ApplicationIsSelected());

            this.AddVariableGroupCommand    = new RelayCommand(_ => AddVariableGroup());
            this.RemoveVariableGroupCommand = new RelayCommand(_ => RemoveVariableGroup(), _ => VariableGroupIsSelected());
        }        

        private void ForceApplication()
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ConfirmInstallAppOnAppServerMessage, this.SelectedApplicationWithOverrideGroup, this.SelectedApplicationServer);

            if (!UserChoosesYes(message)) { return; }

            LogMessageLogic.SaveLogMessage(string.Format(CultureInfo.CurrentCulture,
                "{0} selected to be installed.",
                this.SelectedApplicationWithOverrideGroup));

            this.SelectedApplicationServer.ApplicationWithGroupToForceInstall = this.SelectedApplicationWithOverrideGroup;
            
            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.AppWillBeInstalledOnAppServer, this.SelectedApplicationWithOverrideGroup, this.SelectedApplicationServer);
        }                   

        private void AddServer()
        {
            string newServerName = "New server - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            this.ApplicationServers.Add(new ApplicationServer() { Name = newServerName });

            this.SelectedApplicationServer = this.ApplicationServers.Where(server => server.Name == newServerName).FirstOrDefault();
        }

        private bool AppServerIsSelected()
        {
            return this.SelectedApplicationServer != null;
        }

        private void DeleteServer()
        {
            if (!UserConfirmsDelete(this.SelectedApplicationServer.Name)) { return; }

            LogicBase.Delete<ApplicationServer>(this.SelectedApplicationServer);

            this.ApplicationServers.Remove(this.SelectedApplicationServer);
        }

        private void SaveServer()
        {
            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedApplicationServer.Name);
        }   

        private void AddApplication()
        {
            ApplicationSelectorViewModel viewModel = new ApplicationSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(new ApplicationWithOverrideVariableGroup() { Application = viewModel.SelectedApplication });

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }

        private void EditApplication()
        {
            CustomVariableGroupSelectorViewModel viewModel = new CustomVariableGroupSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplicationWithOverrideGroup.CustomVariableGroup = viewModel.SelectedCustomVariableGroup;

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }

        private bool ApplicationIsSelected()
        {
            return this.SelectedApplicationWithOverrideGroup != null;
        }

        private void RemoveApplication()
        {
            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Remove(this.SelectedApplicationWithOverrideGroup);

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }                

        private void AddVariableGroup()
        {
            CustomVariableGroupSelectorViewModel viewModel = new CustomVariableGroupSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            // Servers shouldn't reference custom variable groups that are associated with an application.
            if (viewModel.SelectedCustomVariableGroup.Application != null)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.CannotUseGroup;
                return;
            }

            this.SelectedApplicationServer.CustomVariableGroups.Add(viewModel.SelectedCustomVariableGroup);

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }

        private bool VariableGroupIsSelected()
        {
            return this.SelectedCustomVariableGroup != null;
        }     

        private void RemoveVariableGroup()
        {
            this.SelectedApplicationServer.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);

            LogicBase.Save<ApplicationServer>(this.SelectedApplicationServer);
        }     

        private void LoadApplicationServers()
        {
            try
            {
                this.ApplicationServers = new ObservableCollection<ApplicationServer>(ApplicationServerLogic.GetAll().ToList());
            }
            catch (SocketException ex)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.DatabaseConnectionFailureMessage;
                LogUtility.LogException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.DatabaseInvalidOperation;
                LogUtility.LogException(ex);
            }
        }
    }
}
