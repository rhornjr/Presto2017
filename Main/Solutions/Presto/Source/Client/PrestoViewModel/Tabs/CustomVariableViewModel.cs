using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableViewModel : ViewModelBase
    {
        private Collection<CustomVariableGroup> _customVariableGroups;
        private CustomVariableGroup _selectedCustomVariableGroup;

        /// <summary>
        /// Gets or sets the custom variable groups.
        /// </summary>
        /// <value>
        /// The custom variable groups.
        /// </value>
        public Collection<CustomVariableGroup> CustomVariableGroups
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

            LoadCustomVariableGroups();
        }

        private void LoadCustomVariableGroups()
        {
            try
            {
                IObjectContainer db = CommonUtility.GetDatabase();

                IEnumerable<CustomVariableGroup> groups = from CustomVariableGroup variableGroup in db
                                                          select variableGroup;

                this.CustomVariableGroups = new Collection<CustomVariableGroup>(groups.ToList());

                db.Close();
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
