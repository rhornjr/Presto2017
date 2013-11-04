using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
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

                if (_selectedAdGroupWithRoles == null) { return; }

                // Every time the group changes, set the observable collection of roles
                _prestoRoles = new PrestoObservableCollection<PrestoRole>();
                if (_selectedAdGroupWithRoles.PrestoRoles != null) { _prestoRoles.AddRange(_selectedAdGroupWithRoles.PrestoRoles); }
                this.NotifyPropertyChanged(() => this.PrestoRoles);
                this.SelectedPrestoRole = null;
            }
        }

        private PrestoObservableCollection<PrestoRole> _prestoRoles;

        // Why I created a separate property: http://stackoverflow.com/a/17788086/279516
        public PrestoObservableCollection<PrestoRole> PrestoRoles
        {
            get { return _prestoRoles; }
        }

        public PrestoRole? SelectedPrestoRole { get; set; }

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

        public ICommand SaveAdInfoCommand { get; private set; }
        public ICommand EncryptCommand { get; set; }

        public ActiveDirectoryInfo ActiveDirectoryInfo { get; set; }

        public bool UserCanModifySecurity
        {
            get
            {
                if (ActiveDirectoryInfo == null) { return false; }  // may not be set yet
                if (ActiveDirectoryInfo.SecurityEnabled == false) { return true; }

                // Security is enabled. Only allow admins to modify.
                return ViewModelUtility.UserIsPrestoAdmin;
            }
        }

        public bool UserCanModifyRoles
        {
            get { return UserCanModifySecurity && AdGroupIsSelected; }
        }

        public SecurityViewModel()
        {
            Debug.WriteLine("SecurityViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadAdInfo();
            LoadAdGroupWithRolesList();

            Debug.WriteLine("SecurityViewModel constructor end " + DateTime.Now);
        }

        private async Task LoadAdInfo()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var prestoWcf = new PrestoWcf<ISecurityService>())
                    {
                        this.ActiveDirectoryInfo = prestoWcf.Service.GetActiveDirectoryInfo();
                    }
                    if (this.ActiveDirectoryInfo == null) { this.ActiveDirectoryInfo = new ActiveDirectoryInfo(); }
                    this.NotifyPropertyChanged(() => this.ActiveDirectoryInfo);
                });
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
            }
        }

        private async Task LoadAdGroupWithRolesList()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var prestoWcf = new PrestoWcf<ISecurityService>())
                    {
                        this.AdGroupWithRolesList = prestoWcf.Service.GetAllAdGroupWithRoles().ToList();
                    }
                });
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

            this.SaveAdInfoCommand = new RelayCommand(SaveAdInfo);
            this.EncryptCommand = new RelayCommand(Encrypt);
        }

        private void SaveAdInfo()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<ISecurityService>())
                {
                    this.ActiveDirectoryInfo = prestoWcf.Service.SaveActiveDirectoryInfo(this.ActiveDirectoryInfo);
                }
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);
                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.ActiveDirectoryInfo.Id));
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
                    this.SelectedAdGroupWithRoles = prestoWcf.Service.SaveAdGroupWithRoles(this.SelectedAdGroupWithRoles);
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

        private void DeleteGroup()
        {
            if (!UserConfirmsDelete(this.SelectedAdGroupWithRoles.AdGroupName)) { return; }

            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.Delete(this.SelectedAdGroupWithRoles);
            }

            this.AdGroupWithRolesList.Remove(this.SelectedAdGroupWithRoles);

            ViewModelUtility.MainWindowViewModel.AddUserMessage("AD group deleted.");

            RefreshGroupList();
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
            this.PrestoRoles.Add(viewModel.SelectedRole);
            this.SaveGroup();
        }

        private void RemoveRole()
        {
            if (!UserConfirmsDelete(this.SelectedPrestoRole.ToString())) { return; }

            this.SelectedAdGroupWithRoles.PrestoRoles.Remove((PrestoRole)SelectedPrestoRole);
            this.PrestoRoles.Remove((PrestoRole)SelectedPrestoRole);

            SaveGroup();
        }

        private bool ExactlyOneRoleIsSelected()
        {
            return this.SelectedPrestoRole != null;
        }

        private void Encrypt()
        {
            ActiveDirectoryInfo.ActiveDirectoryAccountPassword = AesCrypto.Encrypt(ActiveDirectoryInfo.ActiveDirectoryAccountPassword);
            this.NotifyPropertyChanged(() => ActiveDirectoryInfo);
        }
    }
}
