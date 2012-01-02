using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;
using Raven.Abstractions.Exceptions;

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
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _selectedApplicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>();

        /// <summary>
        /// Gets the add server command.
        /// </summary>
        public ICommand AddServerCommand { get; private set; }

        /// <summary>
        /// Gets the delete server command.
        /// </summary>
        public ICommand DeleteServerCommand { get; private set; }

        /// <summary>
        /// Gets the refresh servers command.
        /// </summary>
        public ICommand RefreshServersCommand { get; private set; }

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
        /// Gets the import application command.
        /// </summary>
        public ICommand ImportApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the export application command.
        /// </summary>
        public ICommand ExportApplicationCommand { get; private set; }

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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ObservableCollection<ApplicationWithOverrideVariableGroup> SelectedApplicationsWithOverrideGroup
        {
            get { return this._selectedApplicationsWithOverrideGroup; }

            set
            {
                this._selectedApplicationsWithOverrideGroup = value;
                this.NotifyPropertyChanged(() => this.SelectedApplicationsWithOverrideGroup);
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
            this.AddServerCommand      = new RelayCommand(_ => AddServer());
            this.DeleteServerCommand   = new RelayCommand(_ => DeleteServer(), _ => AppServerIsSelected());
            this.SaveServerCommand     = new RelayCommand(_ => SaveServer(), _ => AppServerIsSelected());
            this.RefreshServersCommand = new RelayCommand(_ => RefreshServers());

            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication());
            this.EditApplicationCommand   = new RelayCommand(_ => EditApplication(), _ => ExactlyOneApplicationIsSelected());
            this.RemoveApplicationCommand = new RelayCommand(_ => RemoveApplication(), _ => ExactlyOneApplicationIsSelected());
            this.ImportApplicationCommand = new RelayCommand(_ => ImportApplication(), _ => AppServerIsSelected());
            this.ExportApplicationCommand = new RelayCommand(_ => ExportApplication(), _ => AtLeastOneApplicationIsSelected());
            this.ForceApplicationCommand  = new RelayCommand(_ => ForceApplication(), _ => ExactlyOneApplicationIsSelected());

            this.AddVariableGroupCommand    = new RelayCommand(_ => AddVariableGroup());
            this.RemoveVariableGroupCommand = new RelayCommand(_ => RemoveVariableGroup(), _ => VariableGroupIsSelected());
        }        

        private void ExportApplication()
        {
            string filePathAndName = SaveFilePathAndNameFromUser(".AppsWithGroup");

            if (filePathAndName == null) { return; }

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ApplicationWithOverrideVariableGroup>));
                xmlSerializer.Serialize(fileStream, this.SelectedApplicationsWithOverrideGroup.ToList());
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, filePathAndName);
        }

        private void ImportApplication()
        {
            string filePathAndName = GetFilePathAndNameFromUser();

            if (string.IsNullOrWhiteSpace(filePathAndName)) { return; }

            List<ApplicationWithOverrideVariableGroup> applicationsWithOverrideVariableGroup;

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ApplicationWithOverrideVariableGroup>));
                applicationsWithOverrideVariableGroup = xmlSerializer.Deserialize(fileStream) as List<ApplicationWithOverrideVariableGroup>;
            }

            foreach (ApplicationWithOverrideVariableGroup group in applicationsWithOverrideVariableGroup)
            {
                ApplicationWithOverrideVariableGroup groupFromDatabase =
                    ApplicationWithOverrideVariableGroupLogic.GetByAppNameAndGroupName(group.Application.Name, group.CustomVariableGroup.Name);
                this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(groupFromDatabase);
            }

            SaveServer();
        }        

        private void ForceApplication()
        {
            ApplicationWithOverrideVariableGroup selectedAppWithGroup = GetSelectedAppWithGroupWhereOnlyOneIsSelected();

            if (selectedAppWithGroup == null) { return; }

            string message = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ConfirmInstallAppOnAppServerMessage, selectedAppWithGroup, this.SelectedApplicationServer);

            if (!UserChoosesYes(message)) { return; }

            LogMessageLogic.SaveLogMessage(string.Format(CultureInfo.CurrentCulture,
                "{0} selected to be installed on {1}.",
                selectedAppWithGroup,
                this.SelectedApplicationServer));

            this.SelectedApplicationServer.ApplicationWithGroupToForceInstall = selectedAppWithGroup;

            SaveServer();

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.AppWillBeInstalledOnAppServer, selectedAppWithGroup, this.SelectedApplicationServer);
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

            LogicBase.Delete(this.SelectedApplicationServer);

            this.ApplicationServers.Remove(this.SelectedApplicationServer);
        }

        private void RefreshServers()
        {
            this.LoadApplicationServers();

            ViewModelUtility.MainWindowViewModel.UserMessage = "Items refreshed.";
        }

        private bool SaveServer()
        {                        
            try
            {
                LogicBase.Save(this.SelectedApplicationServer);
            }
            catch (ConcurrencyException)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                    ViewModelResources.ItemCannotBeSavedConcurrency, this.SelectedApplicationServer.Name);
                return false;
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedApplicationServer.Name);

            return true;
        }   

        private void AddApplication()
        {
            ApplicationWithGroupViewModel viewModel = new ApplicationWithGroupViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(viewModel.ApplicationWithGroup);

            SaveServer();
        }

        private void EditApplication()
        {
            ApplicationWithOverrideVariableGroup selectedAppWithGroup = GetSelectedAppWithGroupWhereOnlyOneIsSelected();

            if (selectedAppWithGroup == null) { return; }

            ApplicationWithGroupViewModel viewModel = new ApplicationWithGroupViewModel(selectedAppWithGroup);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            SaveServer();
        }

        private bool ExactlyOneApplicationIsSelected()
        {
            return this.SelectedApplicationsWithOverrideGroup != null && this.SelectedApplicationsWithOverrideGroup.Count == 1;
        }

        private bool AtLeastOneApplicationIsSelected()
        {
            return this.SelectedApplicationsWithOverrideGroup != null && this.SelectedApplicationsWithOverrideGroup.Count >= 1;
        }

        private void RemoveApplication()
        {
            ApplicationWithOverrideVariableGroup selectedAppWithGroup = GetSelectedAppWithGroupWhereOnlyOneIsSelected();

            if (selectedAppWithGroup == null) { return; }

            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Remove(selectedAppWithGroup);

            SaveServer();
        }

        private ApplicationWithOverrideVariableGroup GetSelectedAppWithGroupWhereOnlyOneIsSelected()
        {
            if (this.SelectedApplicationsWithOverrideGroup == null || this.SelectedApplicationsWithOverrideGroup.Count != 1)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = "To perform the requested action, please select exactly one row.";
                return null;
            }
            
            return this.SelectedApplicationsWithOverrideGroup[0];
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

            SaveServer();
        }

        private bool VariableGroupIsSelected()
        {
            return this.SelectedCustomVariableGroup != null;
        }     

        private void RemoveVariableGroup()
        {
            this.SelectedApplicationServer.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);

            SaveServer();
        }     

        private void LoadApplicationServers()
        {
            try
            {
                this.ApplicationServers = new ObservableCollection<ApplicationServer>(ApplicationServerLogic.GetAll().ToList());
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = "Could not load form. Please see log for details.";
            }
        }
    }
}
