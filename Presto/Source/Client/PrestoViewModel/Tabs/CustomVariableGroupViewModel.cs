using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using System.Xml.Serialization;
using PrestoCommon.Entities;
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
    public class CustomVariableGroupViewModel : ViewModelBase
    {
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private CustomVariableGroup _selectedCustomVariableGroup;

        /// <summary>
        /// Gets the add application command.
        /// </summary>
        public ICommand AddApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the remove application command.
        /// </summary>
        public ICommand RemoveApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the add variable group command.
        /// </summary>
        public ICommand AddVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the delete variable group command.
        /// </summary>
        public ICommand DeleteVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the import variable group command.
        /// </summary>
        public ICommand ImportVariableGroupCommand { get; private set; }        

        /// <summary>
        /// Gets the export variable group command.
        /// </summary>
        public ICommand ExportVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the refresh variable group command.
        /// </summary>
        public ICommand RefreshVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the save group command.
        /// </summary>
        public ICommand SaveVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the add variable command.
        /// </summary>
        public ICommand AddVariableCommand { get; private set; }

        /// <summary>
        /// Gets the edit variable command.
        /// </summary>
        public ICommand EditVariableCommand { get; private set; }

        /// <summary>
        /// Gets the delete variable command.
        /// </summary>
        public ICommand DeleteVariableCommand { get; private set; }

        /// <summary>
        /// Gets or sets the custom variable groups.
        /// </summary>
        /// <value>
        /// The custom variable groups.
        /// </value>
        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get { return this._customVariableGroups; }

            private set
            {
                this._customVariableGroups = value;
                NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        /// <summary>
        /// Gets or sets the selected custom variable group.
        /// </summary>
        /// <value>
        /// The selected custom variable group.
        /// </value>
        public CustomVariableGroup SelectedCustomVariableGroup
        {
            get { return this._selectedCustomVariableGroup; }

            set
            {
                this._selectedCustomVariableGroup = value;
                NotifyPropertyChanged(() => this.SelectedCustomVariableGroup);
            }
        }

        /// <summary>
        /// Gets or sets the selected custom variable.
        /// </summary>
        /// <value>
        /// The selected custom variable.
        /// </value>
        public CustomVariable SelectedCustomVariable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableGroupViewModel"/> class.
        /// </summary>
        public CustomVariableGroupViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadCustomVariableGroups();
        }

        private void Initialize()
        {
            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication(), _ => CustomVariableGroupIsSelected());
            this.RemoveApplicationCommand = new RelayCommand(_ => RemoveApplication(), _ => CustomVariableGroupIsSelected());

            this.AddVariableGroupCommand     = new RelayCommand(_ => AddVariableGroup());
            this.DeleteVariableGroupCommand  = new RelayCommand(_ => DeleteVariableGroup(), _ => CustomVariableGroupIsSelected());
            this.SaveVariableGroupCommand    = new RelayCommand(_ => SaveVariableGroup(), _ => CustomVariableGroupIsSelected());
            this.ExportVariableGroupCommand  = new RelayCommand(_ => ExportVariableGroup(), _ => CustomVariableGroupIsSelected());
            this.ImportVariableGroupCommand  = new RelayCommand(_ => ImportVariableGroup());
            this.RefreshVariableGroupCommand = new RelayCommand(_ => RefreshVariableGroup());

            this.AddVariableCommand    = new RelayCommand(_ => AddVariable());
            this.EditVariableCommand   = new RelayCommand(_ => EditVariable());
            this.DeleteVariableCommand = new RelayCommand(_ => DeleteVariable(), _ => CustomVariableIsSelected());
        }             

        private void AddApplication()
        {
            // ToDo: Need to check first to see if the selected custom variable group is associated with an app server.
            //       If so, don't do this. A group can only be associated with an app or server, not both.

            ApplicationSelectorViewModel viewModel = new ApplicationSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedCustomVariableGroup.Application = viewModel.SelectedApplication;
        }

        private void RemoveApplication()
        {
            this.SelectedCustomVariableGroup.Application = null;
        }

        private void AddVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedCustomVariableGroup.CustomVariables.Add(viewModel.CustomVariable);

            if (SaveVariableGroup() == false)
            {
                this.SelectedCustomVariableGroup.CustomVariables.Remove(viewModel.CustomVariable);
            }
        }

        private void EditVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel(this.SelectedCustomVariable);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            SaveVariableGroup();
        }

        private bool CustomVariableIsSelected()
        {
            return this.SelectedCustomVariable != null;
        }

        private void DeleteVariable()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariable.Key)) { return; }

            CustomVariable selectedCustomVariable = this.SelectedCustomVariable;

            this.SelectedCustomVariableGroup.CustomVariables.Remove(selectedCustomVariable);

            // Need to save the group. If we don't, then we'll get an exception when we access it again.
            // The group still thinks is has this variable until we save the group.
            if (SaveVariableGroup() == false)
            {
                this.SelectedCustomVariableGroup.CustomVariables.Add(selectedCustomVariable);
            }
        }

        private void AddVariableGroup()
        {
            string newGroupName = "New group - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            this.CustomVariableGroups.Add(new CustomVariableGroup() { Name = newGroupName });

            this.SelectedCustomVariableGroup = this.CustomVariableGroups.Where(group => group.Name == newGroupName).FirstOrDefault();
        }

        private bool CustomVariableGroupIsSelected()
        {
            return this.SelectedCustomVariableGroup != null;
        }

        private void DeleteVariableGroup()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariableGroup.Name)) { return; }

            LogicBase.Delete<CustomVariableGroup>(this.SelectedCustomVariableGroup);

            this.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);
        }

        private bool SaveVariableGroup()
        {
            try
            {
                LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);
            }
            catch (ConcurrencyException)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                    ViewModelResources.ItemCannotBeSavedConcurrency, this.SelectedCustomVariableGroup.Name);
                return false;
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedCustomVariableGroup.Name);

            return true;
        }

        private void ImportVariableGroup()
        {
            string filePathAndName = GetFilePathAndNameFromUser();

            if (string.IsNullOrWhiteSpace(filePathAndName)) { return; }

            CustomVariableGroup customVariableGroup;

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomVariableGroup));
                customVariableGroup = xmlSerializer.Deserialize(fileStream) as CustomVariableGroup;
            }

            LogicBase.Save(customVariableGroup);            

            this.CustomVariableGroups.Add(customVariableGroup);
        }

        private void ExportVariableGroup()
        {
            string filePathAndName = SaveFilePathAndNameFromUser(this.SelectedCustomVariableGroup.Name + ".CustomVariableGroup");

            if (filePathAndName == null) { return; }

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomVariableGroup));
                xmlSerializer.Serialize(fileStream, this.SelectedCustomVariableGroup);
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, this.SelectedCustomVariableGroup.Name);
        }

        private void RefreshVariableGroup()
        {
            this.LoadCustomVariableGroups();
        }   

        private void LoadCustomVariableGroups()
        {            
            try
            {
                this.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>(CustomVariableGroupLogic.GetAll().ToList());
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
