using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableGroupSelectorViewModel : ViewModelBase
    {
        private bool _allowMultipleSelections;
        private Collection<CustomVariableGroup> _customVariableGroups;
        private List<CustomVariableGroup> _selectedCustomVariableGroups = new List<CustomVariableGroup>();

        /// <summary>
        /// Gets the add command.
        /// </summary>
        public ICommand AddCommand { get; private set; }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; private set; }

        /// <summary>
        /// Gets the custom variable groups.
        /// </summary>
        public Collection<CustomVariableGroup> CustomVariableGroups
        {
            get { return this._customVariableGroups; }

            private set
            {
                this._customVariableGroups = value;
                this.NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        /// <summary>
        /// Gets or sets the selected custom variable group.
        /// </summary>
        /// <value>
        /// The selected custom variable group.
        /// </value>
        public List<CustomVariableGroup> SelectedCustomVariableGroups
        {
            get { return this._selectedCustomVariableGroups; }

            set
            {
                this._selectedCustomVariableGroups = value;
                this.NotifyPropertyChanged(() => this.SelectedCustomVariableGroups);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableGroupSelectorViewModel"/> class.
        /// </summary>
        public CustomVariableGroupSelectorViewModel(bool allowMultipleSelections)
        {
            // ToDo: This constructor should take a list of existing custom variable groups that are already associated
            //       with a server so we don't display them.
            this._allowMultipleSelections = allowMultipleSelections;
            Initialize();
        }

        private void Initialize()
        {
            this.AddCommand    = new RelayCommand(_ => Add(), _ => CanAdd());
            this.CancelCommand = new RelayCommand(_ => Cancel());

            LoadCustomVariableGroups();
        }

        private bool CanAdd()
        {
            if (this.SelectedCustomVariableGroups == null) { return false; }

            if (this._allowMultipleSelections)
            {
                return this.SelectedCustomVariableGroups.Count >= 1;
            }

            // Only allow one selected item.
            return this.SelectedCustomVariableGroups.Count == 1;
        }

        private void Add()
        {
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void LoadCustomVariableGroups()
        {
            try
            {
                this.CustomVariableGroups = new Collection<CustomVariableGroup>(CustomVariableGroupLogic.GetAll().OrderBy(x => x.Name).ToList());
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
