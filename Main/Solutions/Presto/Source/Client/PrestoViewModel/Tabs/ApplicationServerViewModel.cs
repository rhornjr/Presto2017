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
using PrestoCommon.EntityHelperClasses;
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
        private PrestoObservableCollection<ApplicationServer> _applicationServers = new PrestoObservableCollection<ApplicationServer>();
        private ApplicationServer _selectedApplicationServer;
        private ObservableCollection<ApplicationWithOverrideVariableGroup> _selectedApplicationsWithOverrideGroup = new ObservableCollection<ApplicationWithOverrideVariableGroup>();

        /// <summary>
        /// Gets a value indicating whether [app server is selected].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [app server is selected]; otherwise, <c>false</c>.
        /// </value>
        public bool AppServerIsSelected
        {
            get { return this.SelectedApplicationServer != null; }
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
            this.DeleteServerCommand   = new RelayCommand(_ => DeleteServer(), _ => AppServerIsSelected);
            this.SaveServerCommand     = new RelayCommand(_ => SaveServer(), _ => AppServerIsSelected);
            this.RefreshServersCommand = new RelayCommand(_ => RefreshServers());

            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication());
            this.EditApplicationCommand   = new RelayCommand(_ => EditApplication(), _ => ExactlyOneApplicationIsSelected());
            this.RemoveApplicationCommand = new RelayCommand(_ => RemoveApplication(), _ => ExactlyOneApplicationIsSelected());
            this.ImportApplicationCommand = new RelayCommand(_ => ImportApplication(), _ => AppServerIsSelected);
            this.ExportApplicationCommand = new RelayCommand(_ => ExportApplication(), _ => AtLeastOneApplicationIsSelected());
            this.ForceApplicationCommand  = new RelayCommand(_ => ForceApplication(), _ => AtLeastOneApplicationIsSelectedAndAllAreEnabled());

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
                try
                {
                    applicationsWithOverrideVariableGroup = xmlSerializer.Deserialize(fileStream) as List<ApplicationWithOverrideVariableGroup>;
                }
                catch (Exception ex)
                {
                    ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                        ViewModelResources.CannotImport);
                    LogUtility.LogException(ex);
                    return;
                }
            }

            // When importing, get the apps and custom variable groups from the DB. We'll use those to populate the properties.            
            foreach (ApplicationWithOverrideVariableGroup importedGroup in applicationsWithOverrideVariableGroup)
            {
                importedGroup.Enabled = false;  // default

                Application appFromDb = ApplicationLogic.GetByName(importedGroup.Application.Name);

                if (appFromDb == null)
                {
                    CannotImportGroup(importedGroup);
                    return;
                }

                importedGroup.Application = appFromDb;

                if (importedGroup.CustomVariableGroup != null)
                {
                    CustomVariableGroup groupFromDb = CustomVariableGroupLogic.GetByName(importedGroup.CustomVariableGroup.Name);
                    importedGroup.CustomVariableGroup = groupFromDb;
                }

                this.SelectedApplicationServer.ApplicationsWithOverrideGroup.Add(importedGroup);
            }

            SaveServer();
        }

        private static void CannotImportGroup(ApplicationWithOverrideVariableGroup importedGroup)
        {
            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                "{0} could not be imported because the application ({1}) with which it is associated does not exist.",
                importedGroup.ToString(),
                importedGroup.Application.Name);
        }        

        private void ForceApplication()
        {
            string allAppWithGroupNames = string.Join(",", this.SelectedApplicationsWithOverrideGroup);

            string message = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ConfirmInstallAppOnAppServerMessage, allAppWithGroupNames, this.SelectedApplicationServer);

            if (!UserChoosesYes(message)) { return; }            

            this.SelectedApplicationServer.ApplicationWithGroupToForceInstallList.AddRange(this.SelectedApplicationsWithOverrideGroup);

            if (SaveServer() == false) { return; }

            LogMessageLogic.SaveLogMessage(string.Format(CultureInfo.CurrentCulture,
                "{0} selected to be installed on {1}.",
                allAppWithGroupNames,
                this.SelectedApplicationServer));

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.AppWillBeInstalledOnAppServer, allAppWithGroupNames, this.SelectedApplicationServer);
        }                   

        private void AddServer()
        {
            string newServerName = "New server - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            this.ApplicationServers.Add(new ApplicationServer() { Name = newServerName });

            this.SelectedApplicationServer = this.ApplicationServers.Where(server => server.Name == newServerName).FirstOrDefault();
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
                ApplicationServerLogic.Save(this.SelectedApplicationServer);
            }
            catch (ConcurrencyException)
            {
                string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.ItemCannotBeSavedConcurrency, this.SelectedApplicationServer.Name);

                ViewModelUtility.MainWindowViewModel.UserMessage = message;

                ShowUserMessage(message, ViewModelResources.ItemNotSavedCaption);

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

            LogMessageLogic.SaveLogMessage(message);

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
                LogMessageLogic.SaveLogMessage(message);
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
            ApplicationWithOverrideVariableGroup forceInstallGroup = this.SelectedApplicationServer.GetFromForceInstallList(selectedAppWithGroup);

            if (forceInstallGroup != null) { this.SelectedApplicationServer.ApplicationWithGroupToForceInstallList.Remove(forceInstallGroup); }

            string message = string.Format(CultureInfo.CurrentCulture,
                "{0} was just removed from {1}.",
                selectedAppWithGroup.ToString(), this.SelectedApplicationServer.Name);

            LogMessageLogic.SaveLogMessage(message);

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

        private void LoadApplicationServers()
        {
            try
            {
                // Note: Instead of just setting the entire list to a new list, use Clear() and Add(). When I set
                //       the entire list here, I had to deal with esoteric problems that ruined my day:
                //       http://stackoverflow.com/questions/9438632/nullreferenceexception-using-treeview-and-propertychanged#comment11937214_9438632

                this.ApplicationServers.ClearItemsAndNotifyChangeOnlyWhenDone();

                this.ApplicationServers.AddRange(ApplicationServerLogic.GetAll());
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = "Could not load form. Please see log for details.";
            }
        }
    }
}
