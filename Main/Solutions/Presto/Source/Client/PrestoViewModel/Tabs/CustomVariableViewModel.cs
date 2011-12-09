using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableViewModel : ViewModelBase
    {
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private CustomVariableGroup _selectedCustomVariableGroup;

        /// <summary>
        /// Gets the add variable group command.
        /// </summary>
        public ICommand AddVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the delete variable group command.
        /// </summary>
        public ICommand DeleteVariableGroupCommand { get; private set; }

        /// <summary>
        /// Gets the save group command.
        /// </summary>
        public ICommand SaveGroupCommand { get; private set; }

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
        /// Initializes a new instance of the <see cref="CustomVariableViewModel"/> class.
        /// </summary>
        public CustomVariableViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadCustomVariableGroups();
        }

        private void Initialize()
        {
            this.AddVariableGroupCommand    = new RelayCommand(_ => AddVariableGroup());
            this.DeleteVariableGroupCommand = new RelayCommand(_ => DeleteVariableGroup(), _ => CanDeleteGroup());
            this.SaveGroupCommand           = new RelayCommand(_ => SaveVariableGroup());
        }              

        private void AddVariableGroup()
        {
            string newGroupName = "New group - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            this.CustomVariableGroups.Add(new CustomVariableGroup() { Name = newGroupName });

            this.SelectedCustomVariableGroup = this.CustomVariableGroups.Where(group => group.Name == newGroupName).FirstOrDefault();
        }

        private bool CanDeleteGroup()
        {
            return this.SelectedCustomVariableGroup != null;
        }

        private void DeleteVariableGroup()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariableGroup.Name)) { return; }

            LogicBase.Delete<CustomVariableGroup>(this.SelectedCustomVariableGroup);

            this.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);
        }

        private void SaveVariableGroup()
        {
            LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);
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
