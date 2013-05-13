﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using Xanico.Core;

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
            this.AddCommand    = new RelayCommand(Add, CanAdd);
            this.CancelCommand = new RelayCommand(Cancel);

            this.UserCanceled = true;  // default (do this in case the user closes the window without hitting the cancel button)

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
            this.UserCanceled = false;
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
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    this.CustomVariableGroups =
                        new Collection<CustomVariableGroup>(
                            prestoWcf.Service.GetAllGroups().Where(x => x.Disabled == false).OrderBy(x => x.Name).ToList());
                }
            }
            catch (SocketException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseConnectionFailureMessage);
                Logger.LogException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseInvalidOperation);
                Logger.LogException(ex);
            }
        }        
    }
}
