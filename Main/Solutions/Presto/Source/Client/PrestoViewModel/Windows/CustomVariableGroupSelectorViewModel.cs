using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableGroupSelectorViewModel : ViewModelBase
    {
        private Collection<CustomVariableGroup> _customVariableGroups;
        private PrestoObservableCollection<CustomVariableGroup> _selectedCustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
        private readonly List<CustomVariableGroup> _alreadySelectedCvgs;

        public ICommand AddCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand SetCommand { get; private set; }
        public bool UserCanceled { get; private set; }

        public Collection<CustomVariableGroup> CustomVariableGroups
        {
            get { return this._customVariableGroups; }

            private set
            {
                this._customVariableGroups = value;
                this.NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        public PrestoObservableCollection<CustomVariableGroup> SelectedCustomVariableGroups
        {
            get { return this._selectedCustomVariableGroups; }

            set
            {
                this._selectedCustomVariableGroups = value;
                this.NotifyPropertyChanged(() => this.SelectedCustomVariableGroups);
            }
        }

        public CustomVariableGroupSelectorViewModel(PrestoObservableCollection<CustomVariableGroup> alreadySelectedCvgs)
        {
            Initialize();

            if (alreadySelectedCvgs != null && alreadySelectedCvgs.Count > 0)
            {
                this._alreadySelectedCvgs = new List<CustomVariableGroup>(alreadySelectedCvgs.ToList());
            }
        }

        private void Initialize()
        {
            this.AddCommand    = new RelayCommand(Add, CanAdd);
            this.CancelCommand = new RelayCommand(Cancel);
            this.SetCommand    = new RelayCommand(Set, CanSet);

            this.UserCanceled = true;  // default (do this in case the user closes the window without hitting the cancel button)

            LoadCustomVariableGroups();
        }

        private bool CanAdd()
        {
            if (this.SelectedCustomVariableGroups == null) { return false; }

            return this.SelectedCustomVariableGroups.Count >= 1;
        }

        private void Add()
        {
            this.UserCanceled = false;
            this.Close();
        }

        private bool CanSet()
        {
            return this._alreadySelectedCvgs != null && this._alreadySelectedCvgs.Count > 0;
        }

        private void Set()
        {
            this.SelectedCustomVariableGroups.ClearItemsAndNotifyChangeOnlyWhenDone();

            foreach (var alreadySelectedCvg in this._alreadySelectedCvgs)
            {
                var selectedCvg = this.CustomVariableGroups.FirstOrDefault(x => x.Id == alreadySelectedCvg.Id);
                if (selectedCvg != null)
                {
                    this.SelectedCustomVariableGroups.Add(selectedCvg);
                }
            }
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
                CommonUtility.ProcessException(ex);
            }
            catch (InvalidOperationException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ViewModelResources.DatabaseInvalidOperation);
                CommonUtility.ProcessException(ex);
            }
        }        
    }
}
