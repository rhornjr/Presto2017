using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Tabs
{
    public class SecurityViewModel : ViewModelBase
    {
        private List<AdGroupWithRoles> _adGroupWithRolesList;
        private AdGroupWithRoles _selectedAdGroupWithRoles;

        public List<AdGroupWithRoles> AdGroupWithRolesList
        {
            get { return this._adGroupWithRolesList; }

            private set
            {
                this._adGroupWithRolesList = value;
                this.NotifyPropertyChanged(() => this.AdGroupWithRolesList);
            }
        }

        public AdGroupWithRoles SelectedAdGroupWithRoles
        {
            get { return _selectedAdGroupWithRoles; }
            set
            {
                _selectedAdGroupWithRoles = value;
                this.NotifyPropertyChanged(() => this.SelectedAdGroupWithRoles);
                this.NotifyPropertyChanged(() => this.AdGroupIsSelected);
            }
        }

        public PrestoRole SelectedPrestoRole { get; set; }

        public bool AdGroupIsSelected
        {
            get { return GroupIsSelectedMethod(); }
        }

        public ICommand SaveGroupCommand { get; private set; }
        public ICommand AddGroupCommand { get; private set; }
        public ICommand DeleteGroupCommand { get; private set; }
        public ICommand RefreshGroupCommand { get; private set; }

        public ICommand AddRoleCommand { get; private set; }
        public ICommand RemoveRoleCommand { get; private set; }

        public SecurityViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadAdGroupWithRolesList();
        }

        private void LoadAdGroupWithRolesList()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ISecurityService>())
                {
                    this.AdGroupWithRolesList = prestoWcf.Service.GetAllAdGroupWithRoles().ToList();
                }
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }

        private void Initialize()
        {
            this.SaveGroupCommand    = new RelayCommand(_ => SaveGroup(), GroupIsSelectedMethod);
            this.AddGroupCommand     = new RelayCommand(AddGroup);
            this.DeleteGroupCommand  = new RelayCommand(DeleteGroup, GroupIsSelectedMethod);
            this.RefreshGroupCommand = new RelayCommand(RefreshGroupList);

            this.AddRoleCommand    = new RelayCommand(AddRole);
            this.RemoveRoleCommand = new RelayCommand(RemoveRole, ExactlyOneRoleIsSelected);
        }

        private bool GroupIsSelectedMethod()
        {
            return this.SelectedAdGroupWithRoles != null;
        }

        private void SaveGroup()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ISecurityService>())
                {
                    prestoWcf.Service.SaveAdGroupWithRoles(this.SelectedAdGroupWithRoles);
                }
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);
                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedAdGroupWithRoles.AdGroupName));
        }

        private void AddGroup()
        {
            string newGroupName = "Group " + Guid.NewGuid();

            var group = new AdGroupWithRoles();
            group.AdGroupName = newGroupName;

            this.AdGroupWithRolesList.Add(group);

            this.SelectedAdGroupWithRoles = this.AdGroupWithRolesList.FirstOrDefault(x => x.AdGroupName == newGroupName);
        }

        private void DeleteGroup(object obj)
        {
            throw new System.NotImplementedException();
        }

        private void RefreshGroupList()
        {
            this.LoadAdGroupWithRolesList();

            ViewModelUtility.MainWindowViewModel.AddUserMessage("AD groups refreshed.");
        }

        private void AddRole()
        {
            var viewModel = new RoleSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            if (this.SelectedAdGroupWithRoles.PrestoRoles == null) { this.SelectedAdGroupWithRoles.PrestoRoles = new List<PrestoRole>(); }

            this.SelectedAdGroupWithRoles.PrestoRoles.Add(viewModel.SelectedRole);
        }

        private void RemoveRole(object obj)
        {
            throw new System.NotImplementedException();
        }

        private bool ExactlyOneRoleIsSelected()
        {
            // ToDo: Do this right
            return true;
        }
    }
}
