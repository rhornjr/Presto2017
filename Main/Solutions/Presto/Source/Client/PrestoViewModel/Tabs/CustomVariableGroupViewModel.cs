using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;
using Raven.Abstractions.Exceptions;
using Xanico.Core;

namespace PrestoViewModel.Tabs
{
    public class CustomVariableGroupViewModel : ViewModelBase
    {
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private CustomVariableGroup _selectedCustomVariableGroup;
        private bool _showDisabled;

        public bool CustomVariableGroupIsSelected
        {
            get { return this.CustomVariableGroupIsSelectedMethod(); }
        }

        public ICommand AddVariableGroupCommand { get; private set; }

        public ICommand DeleteVariableGroupCommand { get; private set; }

        public ICommand ImportVariableGroupCommand { get; private set; }        

        public ICommand ExportVariableGroupCommand { get; private set; }

        public ICommand RefreshVariableGroupCommand { get; private set; }

        public ICommand SaveVariableGroupCommand { get; private set; }

        public ICommand AddVariableCommand { get; private set; }

        public ICommand EditVariableCommand { get; private set; }

        public ICommand DeleteVariableCommand { get; private set; }

        public bool ShowDisabled
        {
            get { return this._showDisabled; }

            set
            {
                this._showDisabled = value;
                NotifyPropertyChanged(() => this.ShowDisabled);
            }
        }

        // This is a wrapper around the real CustomVariableGroups so that we can do filtering and sorting.
        // The view binds to this property.
        public ICollectionView CustomVariableGroupsCollectionView { get; private set; }

        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                return this._customVariableGroups;
            }

            private set
            {
                this._customVariableGroups = value;
                NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        public CustomVariableGroup SelectedCustomVariableGroup
        {
            get { return this._selectedCustomVariableGroup; }

            set
            {
                this._selectedCustomVariableGroup = value;
                NotifyPropertyChanged(() => this.SelectedCustomVariableGroup);
                NotifyPropertyChanged(() => this.CustomVariableGroupIsSelected);
            }
        }

        public CustomVariable SelectedCustomVariable { get; set; }

        public CustomVariableGroupViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
        }
        
        private void Initialize()
        {
            this.AddVariableGroupCommand     = new RelayCommand(AddVariableGroup);
            this.DeleteVariableGroupCommand  = new RelayCommand(DeleteVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.SaveVariableGroupCommand    = new RelayCommand(_ => SaveVariableGroup(null), CustomVariableGroupIsSelectedMethod);
            this.ExportVariableGroupCommand  = new RelayCommand(ExportVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.ImportVariableGroupCommand  = new RelayCommand(ImportVariableGroup);
            this.RefreshVariableGroupCommand = new RelayCommand(RefreshVariableGroups);

            this.AddVariableCommand    = new RelayCommand(AddVariable);
            this.EditVariableCommand   = new RelayCommand(EditVariable, CustomVariableIsSelected);
            this.DeleteVariableCommand = new RelayCommand(DeleteVariable, CustomVariableIsSelected);

            LoadCustomVariableGroups();

            InitializeCustomVariableGroupsCollectionView();
        }

        private void InitializeCustomVariableGroupsCollectionView()
        {
            this.CustomVariableGroupsCollectionView = CollectionViewSource.GetDefaultView(this.CustomVariableGroups);
            this.CustomVariableGroupsCollectionView.SortDescriptions.Clear();
            this.CustomVariableGroupsCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            ApplyFilterToCustomVariableGroups();

            NotifyPropertyChanged(() => this.CustomVariableGroupsCollectionView);
        }

        private void ApplyFilterToCustomVariableGroups()
        {
            if (_showDisabled)
            {
                this.CustomVariableGroupsCollectionView.Filter = _ => { return true; };
            }
            else
            {
                this.CustomVariableGroupsCollectionView.Filter = (item) =>
                {
                    var myitem = (CustomVariableGroup)item;
                    return !myitem.Disabled;
                };
            }
        }

        // Named this method this way because we have a property of the same name. The RelayCommands need to specify
        // a method, not a property.
        private bool CustomVariableGroupIsSelectedMethod()
        {
            return this.SelectedCustomVariableGroup != null;
        }

        private void AddVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedCustomVariableGroup.CustomVariables.Add(viewModel.CustomVariable);

            SaveVariableGroup(() => this.SelectedCustomVariableGroup.CustomVariables.Remove(viewModel.CustomVariable));
        }

        private void EditVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel(this.SelectedCustomVariable);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            SaveVariableGroup(null);
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
            SaveVariableGroup(() => this.SelectedCustomVariableGroup.CustomVariables.Add(selectedCustomVariable));
        }

        private void AddVariableGroup()
        {
            string newGroupName = "New group - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            CustomVariableGroup newGroup = new CustomVariableGroup() { Name = newGroupName }; 

            this.CustomVariableGroups.Add(newGroup);

            this.SelectedCustomVariableGroup = this.CustomVariableGroups.Where(group => group.Name == newGroupName).FirstOrDefault();
        }

        private void DeleteVariableGroup()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariableGroup.Name)) { return; }

            try
            {
                CustomVariableGroup deletedGroup = this.CustomVariableGroups.Where(x => x.Id == this.SelectedCustomVariableGroup.Id).FirstOrDefault();

                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    prestoWcf.Service.DeleteGroup(deletedGroup);
                }

                this.CustomVariableGroups.Remove(deletedGroup);
            }
            catch (Exception ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);

                ShowUserMessage(ex.Message, ViewModelResources.ItemCannotBeDeletedCaption);

                return;
            }
        }

        private void SaveVariableGroup(Action actionIfSaveFails)
        {
            string selectedGroupName = this.SelectedCustomVariableGroup.Name;

            try
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    this.SelectedCustomVariableGroup = prestoWcf.Service.SaveGroup(this.SelectedCustomVariableGroup);
                }
                InitializeCustomVariableGroupsCollectionView();
            }
            catch (ConcurrencyException)
            {
                string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.ItemCannotBeSavedConcurrency, this.SelectedCustomVariableGroup.Name);

                ViewModelUtility.MainWindowViewModel.AddUserMessage(message);

                ShowUserMessage(message, ViewModelResources.ItemNotSavedCaption);

                actionIfSaveFails.Invoke();
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, selectedGroupName));
        }

        private void ImportVariableGroup()
        {
            string[] filePathAndNames = GetFilePathAndNamesFromUser();

            if (filePathAndNames == null || filePathAndNames.Count() < 1) { return; }

            CustomVariableGroup customVariableGroup;

            using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
            {
                foreach (string filePathAndName in filePathAndNames)
                {
                    using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomVariableGroup));
                        try
                        {
                            customVariableGroup = xmlSerializer.Deserialize(fileStream) as CustomVariableGroup;
                        }
                        catch (Exception ex)
                        {
                            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                                ViewModelResources.CannotImport));
                            Logger.LogException(ex);
                            return;
                        }
                    }

                    // If the variable group name exists, append " - copy" to it.
                    if (this.CustomVariableGroups.Any(group => group.Name == customVariableGroup.Name))
                    {
                        customVariableGroup.Name += " - copy";
                    }

                    customVariableGroup.Id = null;  // This is new.
                    customVariableGroup.Etag = Guid.Empty;

                    prestoWcf.Service.SaveGroup(customVariableGroup);

                    this.CustomVariableGroups.Add(customVariableGroup);
                }
            }
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

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, this.SelectedCustomVariableGroup.Name));
        }

        private void RefreshVariableGroups()
        {
            this.LoadCustomVariableGroups();
            InitializeCustomVariableGroupsCollectionView();

            ViewModelUtility.MainWindowViewModel.AddUserMessage("Variables refreshed.");
        }   

        private void LoadCustomVariableGroups()
        {            
            try
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    this.CustomVariableGroups =
                        new ObservableCollection<CustomVariableGroup>(
                            prestoWcf.Service.GetAllGroups().ToList());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }
    }
}
