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
using PrestoViewModel.Windows;

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

            this.AddVariableGroupCommand    = new RelayCommand(_ => AddVariableGroup());
            this.DeleteVariableGroupCommand = new RelayCommand(_ => DeleteVariableGroup(), _ => CustomVariableGroupIsSelected());
            this.SaveVariableGroupCommand   = new RelayCommand(_ => SaveVariableGroup());

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

            // ToDo: I had to do this for now because I couldn't have CustomVariableGroup derive from NotifyPropertyChangedBase
            //       or I'd get an unsupported hierarchy change with db4o. When I get a chance to start with a fresh DB,
            //       have all the entities derive from EntityBase, and have EntityBase derive from NotifyPropertyChangedBase.
            //       EntityBase will give us a placeholder for future implementation, if necessary.
            //       May want to consider a strategy for dealing with this issue if we need to change the class hierarchy of
            //       something.
            NotifyPropertyChanged(() => this.SelectedCustomVariableGroup);
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

            LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);
        }

        private void EditVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel(this.SelectedCustomVariable);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);
        }

        private bool CustomVariableIsSelected()
        {
            return this.SelectedCustomVariable != null;
        }

        private void DeleteVariable()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariable.Key)) { return; }

            LogicBase.Delete<CustomVariable>(this.SelectedCustomVariable);

            this.SelectedCustomVariableGroup.CustomVariables.Remove(this.SelectedCustomVariable);

            // Need to save the group. If we don't, then we'll get an exception when we access it again.
            // The group still thinks is has this variable until we save the group.
            LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);
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

        private void SaveVariableGroup()
        {
            LogicBase.Save<CustomVariableGroup>(this.SelectedCustomVariableGroup);

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedCustomVariableGroup.Name);
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
