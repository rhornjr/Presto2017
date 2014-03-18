using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Data;
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

namespace PrestoViewModel.Tabs
{
    public class CustomVariableGroupViewModel : ViewModelBase
    {
        private PrestoObservableCollection<CustomVariableGroup> _customVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
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

        public ICommand FindCommand { get; private set; }

        public string VariableKey { get; set; }

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

        public PrestoObservableCollection<CustomVariableGroup> CustomVariableGroups
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
            Debug.WriteLine("CustomVariableGroupViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            Debug.WriteLine("CustomVariableGroupViewModel constructor end " + DateTime.Now);
        }
        
        private async Task Initialize()
        {
            this.AddVariableGroupCommand     = new RelayCommand(AddVariableGroup);
            this.DeleteVariableGroupCommand  = new RelayCommand(DeleteVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.SaveVariableGroupCommand    = new RelayCommand(_ => SaveVariableGroup(null), CustomVariableGroupIsSelectedMethod);
            this.ExportVariableGroupCommand  = new RelayCommand(ExportVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.ImportVariableGroupCommand  = new RelayCommand(ImportVariableGroup);
            this.RefreshVariableGroupCommand = new RelayCommand(RefreshVariableGroups);
            this.FindCommand                 = new RelayCommand(FindCustomVariable);

            this.AddVariableCommand    = new RelayCommand(AddVariable);
            this.EditVariableCommand   = new RelayCommand(EditVariable, CustomVariableIsSelected);
            this.DeleteVariableCommand = new RelayCommand(DeleteVariable, CustomVariableIsSelected);

            await Task.Run(() => LoadCustomVariableGroups());

            // This used to be included in the above await action. But that caused errors because
            // CustomVariableGroupsCollectionView was initialized on a background thread and subsequent
            // updates would cause a threading error.
            InitializeCustomVariableGroupsCollectionView(() => ApplyDisabledFilterToCustomVariableGroups());
        }

        private void FindCustomVariable()
        {
            var variableGroupIds = new List<string>();

            foreach (var variableGroup in this.CustomVariableGroups)
            {
                foreach (var variable in variableGroup.CustomVariables)
                {
                    if (variable.Key == this.VariableKey)
                    {
                        variableGroupIds.Add(variableGroup.Id);
                    }
                }
            }

            InitializeCustomVariableGroupsCollectionView(() => ApplyVariableKeyFilter(variableGroupIds));
        }

        private void ApplyVariableKeyFilter(List<string> variableGroupIds)
        {
            this.CustomVariableGroupsCollectionView.Filter = (item) =>
            {
                var myitem = (CustomVariableGroup)item;
                return variableGroupIds.Contains(myitem.Id);
            };
        }

        private void InitializeCustomVariableGroupsCollectionView(Action applyFilterAction)
        {
            this.CustomVariableGroupsCollectionView = CollectionViewSource.GetDefaultView(this.CustomVariableGroups);
            this.CustomVariableGroupsCollectionView.SortDescriptions.Clear();
            this.CustomVariableGroupsCollectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            applyFilterAction();

            NotifyPropertyChanged(() => this.CustomVariableGroupsCollectionView);
        }

        private void ApplyDisabledFilterToCustomVariableGroups()
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
            string newGroupName = "New group - " + Guid.NewGuid().ToString();

            var newGroup = new CustomVariableGroup() { Name = newGroupName };

            this.SelectedCustomVariableGroup = newGroup;

            SaveVariableGroup(null);

            //this.CustomVariableGroups.Add(newGroup);

            //this.SelectedCustomVariableGroup = this.CustomVariableGroups.FirstOrDefault(group => @group.Name == newGroupName);
        }

        private void DeleteVariableGroup()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariableGroup.Name)) { return; }

            if (string.IsNullOrEmpty(this.SelectedCustomVariableGroup.Id))
            {
                // Group hasn't ever been saved. Just remove it from the list.
                this.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);
                return;
            }

            try
            {
                CustomVariableGroup deletedGroup = this.CustomVariableGroups.FirstOrDefault(x => x.Id == this.SelectedCustomVariableGroup.Id);

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
                    var savedGroup = prestoWcf.Service.SaveGroup(this.SelectedCustomVariableGroup);
                    UpdateCacheWithSavedItem(savedGroup);
                    this.SelectedCustomVariableGroup = savedGroup;
                }
                InitializeCustomVariableGroupsCollectionView(() => ApplyDisabledFilterToCustomVariableGroups());
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);

                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);

                if (actionIfSaveFails != null) { actionIfSaveFails.Invoke(); }

                return;
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, selectedGroupName));
        }

        private void UpdateCacheWithSavedItem(CustomVariableGroup savedGroup)
        {
            int index = this._customVariableGroups.ToList().FindIndex(x => x.Id == savedGroup.Id);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (index >= 0)
                {
                    this._customVariableGroups[index] = savedGroup;
                }
                else
                {
                    this._customVariableGroups.Add(savedGroup);
                }
            });
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
                            CommonUtility.ProcessException(ex);
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

                    customVariableGroup = prestoWcf.Service.SaveGroup(customVariableGroup);

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
            InitializeCustomVariableGroupsCollectionView(() => ApplyDisabledFilterToCustomVariableGroups());

            ViewModelUtility.MainWindowViewModel.AddUserMessage("Variables refreshed.");
        }   

        private void LoadCustomVariableGroups()
        {            
            try
            {
                this.CustomVariableGroups.ClearItemsAndNotifyChangeOnlyWhenDone();
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    this.CustomVariableGroups.AddRange(prestoWcf.Service.GetAllGroups().ToList());
                }
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }
    }
}
