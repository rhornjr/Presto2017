using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;
using Xanico.Core;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationServerViewModel : ViewModelBase
    {
        private List<ApplicationServer> _allServers;
        private InstallationEnvironment _selectedInstallationEnvironment;
        private List<InstallationEnvironment> _installationEnvironments;
        private PrestoObservableCollection<ApplicationServer> _applicationServers = new PrestoObservableCollection<ApplicationServer>();
        private ApplicationServer _selectedApplicationServer;
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _selectedApplicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>();
        private static string _selfUpdatingAppName = GetSelfUpdatingAppName();

        /// <summary>
        /// Gets a value indicating whether [app server is selected].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [app server is selected]; otherwise, <c>false</c>.
        /// </value>
        public bool AppServerIsSelected
        {
            get { return AppServerIsSelectedMethod(); }
        }

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
        public List<InstallationEnvironment> InstallationEnvironments
        {
            get
            {
                if (this._installationEnvironments == null)
                {
                    using (var prestoWcf = new PrestoWcf<IInstallationEnvironmentService>())
                    {
                        this._installationEnvironments =
                            prestoWcf.Service.GetAllInstallationEnvironments().OrderBy(x => x.LogicalOrder).ToList();
                    }
                }
                return this._installationEnvironments;
            }
        }

        /// <summary>
        /// Gets or sets the application servers.
        /// </summary>
        /// <value>
        /// The application servers.
        /// </value>
        public PrestoObservableCollection<ApplicationServer> ApplicationServers
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
                this.NotifyPropertyChanged(() => this.AppServerIsSelected);
                this.NotifyPropertyChanged(() => this.SelectedApplicationServerApplicationsWithOverrideGroup);
                this.NotifyPropertyChanged(() => this.SelectedApplicationServerCustomVariableGroups);
                this.NotifyPropertyChanged(() => this.SelectedApplicationServerInstallationEnvironment);
            }
        }

        public IEnumerable<ApplicationWithOverrideVariableGroup> SelectedApplicationServerApplicationsWithOverrideGroup
        {
            get
            {
                if (this.SelectedApplicationServer == null ||
                    this.SelectedApplicationServer.ApplicationsWithOverrideGroup == null) { return null; }

                return this.SelectedApplicationServer.ApplicationsWithOverrideGroup
                    .OrderBy(x => x.Application.Name)
                    .ThenBy(x => x.CustomVariableGroup == null ? string.Empty : x.CustomVariableGroup.Name);
            }
        }

        public IEnumerable<CustomVariableGroup> SelectedApplicationServerCustomVariableGroups
        {
            get
            {
                if (this.SelectedApplicationServer == null || this.SelectedApplicationServer.CustomVariableGroups == null) { return null; }
                return this.SelectedApplicationServer.CustomVariableGroups.OrderBy(x => x.Name);
            }
        }

        public InstallationEnvironment SelectedApplicationServerInstallationEnvironment
        {
            get
            {
                if (this.SelectedApplicationServer == null || this.SelectedApplicationServer.InstallationEnvironment == null)
                    { return null; }

                return this.InstallationEnvironments.First(x => x.Id == this.SelectedApplicationServer.InstallationEnvironment.Id);
            }

            set
            {
                this.SelectedApplicationServer.InstallationEnvironment = value;
            }
        }

        public InstallationEnvironment SelectedInstallationEnvironment
        {
            get
            {
                if (this._selectedInstallationEnvironment == null)
                {
                    this._selectedInstallationEnvironment = this.InstallationEnvironments.First();
                }
                return this._selectedInstallationEnvironment;
            }

            set
            {
                if (this._selectedInstallationEnvironment == value) { return; }
                this._selectedInstallationEnvironment = value;
                LoadApplicationServers(false);
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
            this.AddServerCommand      = new RelayCommand(AddServer);
            this.DeleteServerCommand   = new RelayCommand(DeleteServer, AppServerIsSelectedMethod);
            this.SaveServerCommand     = new RelayCommand(_ => SaveServer(), AppServerIsSelectedMethod);
            this.RefreshServersCommand = new RelayCommand(RefreshServers);

            this.AddApplicationCommand    = new RelayCommand(AddApplication);
            this.EditApplicationCommand   = new RelayCommand(EditApplication, ExactlyOneApplicationIsSelected);
            this.RemoveApplicationCommand = new RelayCommand(RemoveApplication, ExactlyOneApplicationIsSelected);
            this.ImportApplicationCommand = new RelayCommand(ImportApplication, AppServerIsSelectedMethod);
            this.ExportApplicationCommand = new RelayCommand(ExportApplication, AtLeastOneApplicationIsSelected);
            this.ForceApplicationCommand  = new RelayCommand(ForceApplication, AtLeastOneApplicationIsSelectedAndAllAreEnabled);

            this.AddVariableGroupCommand    = new RelayCommand(AddVariableGroup);
            this.RemoveVariableGroupCommand = new RelayCommand(RemoveVariableGroup, VariableGroupIsSelected);
        }

        // Named this method this way because we have a property of the same name. The RelayCommands need to specify
        // a method, not a property.
        private bool AppServerIsSelectedMethod()
        {
            return this.SelectedApplicationServer != null;
        }

        private void ExportApplication()
        {
            string fileName = SetFileNameBasedOnSelection();

            string filePathAndName = SaveFilePathAndNameFromUser(fileName);

            if (filePathAndName == null) { return; }

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ApplicationWithOverrideVariableGroup>));
                xmlSerializer.Serialize(fileStream, this.SelectedApplicationsWithOverrideGroup.ToList());
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, filePathAndName));
        }

        private string SetFileNameBasedOnSelection()
        {
            // Since the user can choose multiple rows, the default file name changes based on what is selected.
            string fileName = string.Empty; // default
            string fileNameSuffix = ".AppsWithGroup";

            // One row selected:
            if (this.SelectedApplicationsWithOverrideGroup.Count == 1)
            {
                fileName = this.SelectedApplicationsWithOverrideGroup[0].Application.Name;
                if (this.SelectedApplicationsWithOverrideGroup[0].CustomVariableGroup != null)
                {
                    fileName += " and " + this.SelectedApplicationsWithOverrideGroup[0].CustomVariableGroup.Name;
                }
                return fileName += fileNameSuffix;
            }

            // If the number of distinct rows is 1 (all have the same app), then capture that.
            bool allRowsAreForSameApp = false;
            if (this.SelectedApplicationsWithOverrideGroup.Select(x => x.Application.Name).Distinct().Count() == 1) { allRowsAreForSameApp = true; }

            // If the application is the same for all rows, use something like "App 1.2 with multiple override groups".
            if (allRowsAreForSameApp)
            {
                return this.SelectedApplicationsWithOverrideGroup[0].Application.Name + " with multiple override groups" + fileNameSuffix;
            }

            // If we get here, then we have multiple rows selected with different apps in each row. Return a generic file name.
            return "Multiple apps" + fileNameSuffix;
        }

        private void ImportApplication()
        {
            string filePathAndName = GetFilePathAndNameFromUser();

            if (string.IsNullOrWhiteSpace(filePathAndName)) { return; }

            List<ApplicationWithOverrideVariableGroup> applicationsWithOverrideVariableGroup;

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ApplicationWithOverrideVariableGroup>));
                try
                {
                    applicationsWithOverrideVariableGroup = xmlSerializer.Deserialize(fileStream) as List<ApplicationWithOverrideVariableGroup>;
                }
                catch (Exception ex)
                {
                    ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                        ViewModelResources.CannotImport));
                    Logger.LogException(ex);
                    return;
                }
            }

            // When importing, get the apps and custom variable groups from the DB. We'll use those to populate the properties.            
            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                foreach (ApplicationWithOverrideVariableGroup importedGroup in applicationsWithOverrideVariableGroup)
                {
                    importedGroup.Enabled = false;  // default

                    Application appFromDb;

                    appFromDb = prestoWcf.Service.GetByName(importedGroup.Application.Name);

                    if (appFromDb == null)
                    {
                        CannotImportGroup(importedGroup);
                        return;
                    }

                    importedGroup.Application = appFromDb;

                    if (importedGroup.CustomVariableGroup != null)
                    {
                        using (var prestoWcf2 = new PrestoWcf<ICustomVariableGroupService>())
                        {
                            CustomVariableGroup groupFromDb = prestoWcf2.Service.GetCustomVariableGroupByName(importedGroup.CustomVariableGroup.Name);
                            importedGroup.CustomVariableGroup = groupFromDb;
                        }
                    }

                    this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(importedGroup);
                }
            }

            SaveServer();
        }

        private static void CannotImportGroup(ApplicationWithOverrideVariableGroup importedGroup)
        {
            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                "{0} could not be imported because the application ({1}) with which it is associated does not exist.",
                importedGroup.ToString(),
                importedGroup.Application.Name));
        }

        private void ForceApplication()
        {
            if (PrestoUpdaterAppIncludedInForceRequestOfMultipleApps()) { return; }

            // Special installation for the Presto Self-updater app
            if (InstallPrestoUpdaterIfItIsSelected()) { return; }

            string allAppWithGroupNames = string.Join(",", this.SelectedApplicationsWithOverrideGroup);

            string message = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ConfirmInstallAppOnAppServerMessage, allAppWithGroupNames, this.SelectedApplicationServer);

            if (!UserChoosesYes(message)) { return; }

            List<ServerForceInstallation> serverForceInstallations = new List<ServerForceInstallation>();
            foreach (ApplicationWithOverrideVariableGroup appWithGroup in this.SelectedApplicationsWithOverrideGroup)
            {
                ServerForceInstallation serverForceInstallation = new ServerForceInstallation(this.SelectedApplicationServer, appWithGroup);
                serverForceInstallations.Add(serverForceInstallation);
            }
            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                prestoWcf.Service.SaveForceInstallations(serverForceInstallations);
            }

            LogAndShowAppToBeInstalled(allAppWithGroupNames);
        }

        private void LogAndShowAppToBeInstalled(string allAppWithGroupNames)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                                           "{0} selected to be installed on {1}.",
                                           allAppWithGroupNames,
                                           this.SelectedApplicationServer);

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveLogMessage(message);
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.AppWillBeInstalledOnAppServer, allAppWithGroupNames, this.SelectedApplicationServer));
        }

        private bool PrestoUpdaterAppIncludedInForceRequestOfMultipleApps()
        {
            if (this.SelectedApplicationsWithOverrideGroup.Count > 1 &&
                this.SelectedApplicationsWithOverrideGroup.Any(
                    x => x.Application.Name.Equals(_selfUpdatingAppName, StringComparison.OrdinalIgnoreCase)))
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                    "Cannot include the {0} app with a request to install multiple apps. If you want to install the " +
                    "updater app, select it by itself.",
                    _selfUpdatingAppName));
                return true;
            }

            return false;
        }

        private bool InstallPrestoUpdaterIfItIsSelected()
        {
            // If the only app, that is selected, is the Presto self-updater, then return true.

            if (this.SelectedApplicationsWithOverrideGroup.Count == 1 &&
                this.SelectedApplicationsWithOverrideGroup[0].Application.Name.Equals(_selfUpdatingAppName, StringComparison.OrdinalIgnoreCase))
            {
                LogAndShowAppToBeInstalled(_selfUpdatingAppName);

                Task.Factory.StartNew(() =>
                {
                    InstallPrestoUpdater();
                });

                return true;
            }

            return false;
        }

        private static string GetSelfUpdatingAppName()
        {
            return ConfigurationManager.AppSettings["selfUpdatingAppName"];
        }

        private void InstallPrestoUpdater()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    prestoWcf.Service.InstallPrestoSelfUpdater(this.SelectedApplicationServer);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                    ViewModelResources.PrestoUpdaterCouldNotBeInstalled));
            }
        }

        private void AddServer()
        {
            string newServerName = "New server - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            ApplicationServer server = new ApplicationServer();
            server.Name = newServerName;
            //server.InstallationEnvironment = this.InstallationEnvironments[0]; // Use first one as default.

            this.ApplicationServers.Add(server);

            this.SelectedApplicationServer = this.ApplicationServers.Where(x => x.Name == newServerName).FirstOrDefault();
        }        

        private void DeleteServer()
        {
            if (!UserConfirmsDelete(this.SelectedApplicationServer.Name)) { return; }

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.Delete(this.SelectedApplicationServer);
            }

            this.ApplicationServers.Remove(this.SelectedApplicationServer);
        }

        private void RefreshServers()
        {            
            this.LoadApplicationServers();

            ViewModelUtility.MainWindowViewModel.AddUserMessage("Servers refreshed.");
        }

        private bool SaveServer()
        {
            if (this.SelectedApplicationServer.InstallationEnvironment == null)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.ServerNotSavedEnvironmentMissing);
                ShowUserMessage(ViewModelResources.ServerNotSavedEnvironmentMissing, "Missing Environment");
                return false;
            }
              
            try
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    this.SelectedApplicationServer = prestoWcf.Service.SaveServer(this.SelectedApplicationServer);
                    UpdateCacheWithSavedItem(this.SelectedApplicationServer);
                }

                this.NotifyPropertyChanged(() => this.SelectedApplicationServerApplicationsWithOverrideGroup);
                this.NotifyPropertyChanged(() => this.SelectedApplicationServerCustomVariableGroups);
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);

                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);

                return false;
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedApplicationServer.Name));

            return true;
        }

        private void UpdateCacheWithSavedItem(ApplicationServer savedServer)
        {
            int index = this._allServers.FindIndex(x => x.Id == savedServer.Id);

            if (index >= 0)
            {
                this._allServers[index] = savedServer;
            }
            else
            {
                this._allServers.Add(savedServer);
            }
        }

        private void AddApplication()
        {
            ApplicationWithGroupViewModel viewModel = new ApplicationWithGroupViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            if (CommonUtility.GetAppWithGroup(this.SelectedApplicationServer.ApplicationsWithOverrideGroup, viewModel.ApplicationWithGroup) != null)
            {
                // Uh oh. This app with group already exists in the server list. DENIED.                
                string ohNoYouDont = string.Format(CultureInfo.CurrentCulture,
                    "{0} cannot be added to {1} because {1} already contains {0}.",
                    viewModel.ApplicationWithGroup.ToString(), this.SelectedApplicationServer.Name);
                ShowUserMessage(ohNoYouDont, ViewModelResources.ItemNotSavedCaption);
                return;
            }

            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(viewModel.ApplicationWithGroup);

            string message = string.Format(CultureInfo.CurrentCulture,
                "{0} was just added with the enabled property set to {1} on {2}.",
                viewModel.ApplicationWithGroup.ToString(), viewModel.ApplicationWithGroup.Enabled, this.SelectedApplicationServer.Name);

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveLogMessage(message);
            }

            SaveServer();
        }

        private void EditApplication()
        {
            ApplicationWithOverrideVariableGroup selectedAppWithGroup = GetSelectedAppWithGroupWhereOnlyOneIsSelected();

            if (selectedAppWithGroup == null) { return; }

            // If the user changes the enabled property, we need to log that.
            bool originalEnabledValue = selectedAppWithGroup.Enabled;

            ApplicationWithGroupViewModel viewModel = new ApplicationWithGroupViewModel(selectedAppWithGroup);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            if (originalEnabledValue != selectedAppWithGroup.Enabled)
            {
                string message = string.Format(CultureInfo.CurrentCulture,
                    "For {0}, the enabled property was changed from {1} to {2} on {3}",
                    selectedAppWithGroup.ToString(), originalEnabledValue, selectedAppWithGroup.Enabled, this.SelectedApplicationServer.Name);

                using (var prestoWcf = new PrestoWcf<IBaseService>())
                {
                    prestoWcf.Service.SaveLogMessage(message);
                }
            }

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

        private bool AtLeastOneApplicationIsSelectedAndAllAreEnabled()
        {
            if (AtLeastOneApplicationIsSelected() == false) { return false; }

            // If any are false, return false.
            return !this.SelectedApplicationsWithOverrideGroup.Any(x => x.Enabled == false);
        }

        private void RemoveApplication()
        {                       
            ApplicationWithOverrideVariableGroup selectedAppWithGroup = GetSelectedAppWithGroupWhereOnlyOneIsSelected();

            if (!UserConfirmsDelete(selectedAppWithGroup.ToString())) { return; }

            this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Remove(selectedAppWithGroup);

            // If this app group was selected to be force installed, remove it from that list as well.
            ServerForceInstallation forceInstallGroup = this.SelectedApplicationServer.GetFromForceInstallList(selectedAppWithGroup);

            if (forceInstallGroup != null)
            {
                using (var prestoWcf = new PrestoWcf<IServerService>())
                {
                    prestoWcf.Service.RemoveForceInstallation(forceInstallGroup);
                }
            }

            string message = string.Format(CultureInfo.CurrentCulture,
                "{0} was just removed from {1}.",
                selectedAppWithGroup.ToString(), this.SelectedApplicationServer.Name);

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.SaveLogMessage(message);
            }

            SaveServer();
        }

        private ApplicationWithOverrideVariableGroup GetSelectedAppWithGroupWhereOnlyOneIsSelected()
        {
            if (this.SelectedApplicationsWithOverrideGroup == null || this.SelectedApplicationsWithOverrideGroup.Count != 1)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage("To perform the requested action, please select exactly one row.");
                return null;
            }
            
            return this.SelectedApplicationsWithOverrideGroup[0];
        }

        private void AddVariableGroup()
        {
            CustomVariableGroupSelectorViewModel viewModel = new CustomVariableGroupSelectorViewModel(true);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            // ToDo: Need to validate against the new way that groups are associated with an app.
            // Note: The Presto Task Runner will throw an exception if there are duplicates, but it's
            //       still nice to let the user know right now.
            // Servers shouldn't reference custom variable groups that are associated with an application.
            //if (viewModel.SelectedCustomVariableGroups.Any(group => group.Application != null))
            //{
            //    ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.CannotUseGroup;
            //    return;
            //}

            viewModel.SelectedCustomVariableGroups.ForEach(group => this.SelectedApplicationServer.CustomVariableGroups.Add(group));

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

        private void LoadApplicationServers(bool refreshServersFromService = true)
        {
            try
            {
                // Note: Instead of just setting the entire list to a new list, use Clear() and Add(). When I set
                //       the entire list here, I had to deal with esoteric problems that ruined my day:
                //       http://stackoverflow.com/questions/9438632/nullreferenceexception-using-treeview-and-propertychanged#comment11937214_9438632

                this.ApplicationServers.ClearItemsAndNotifyChangeOnlyWhenDone();

                if (refreshServersFromService)
                {
                    using (var prestoWcf = new PrestoWcf<IServerService>())
                    {
                        this._allServers = prestoWcf.Service.GetAllServers().ToList();
                    }
                }

                // Only show the servers for the selected environment.
                this.ApplicationServers.AddRange(
                    _allServers.Where(x => x.InstallationEnvironment.Id == this.SelectedInstallationEnvironment.Id));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }
    }
}
