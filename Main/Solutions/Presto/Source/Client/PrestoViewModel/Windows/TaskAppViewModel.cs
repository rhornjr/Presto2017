using System;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    [TaskTypeAttribute(TaskType.App)]
    public class TaskAppViewModel : TaskViewModel
    {
        private ApplicationWithOverrideVariableGroup _originalAppWithGroup;
        private ApplicationWithOverrideVariableGroup _applicationWithOverrideVariableGroup = new ApplicationWithOverrideVariableGroup();

        /// <summary>
        /// Gets or sets the ok command.
        /// </summary>
        /// <value>
        /// The ok command.
        /// </value>
        public ICommand OkCommand { get; set; }

        /// <summary>
        /// Gets or sets the cancel command.
        /// </summary>
        /// <value>
        /// The cancel command.
        /// </value>
        public ICommand CancelCommand { get; set; }

        public ApplicationWithOverrideVariableGroup ApplicationWithGroup
        {
            get { return this._applicationWithOverrideVariableGroup; }

            set
            {
                this._applicationWithOverrideVariableGroup = value;
                NotifyPropertyChanged(() => this.ApplicationWithGroup);
            }
        }

        /// <summary>
        /// Gets or sets the select application command.
        /// </summary>
        /// <value>
        /// The select application command.
        /// </value>
        public ICommand SelectApplicationCommand { get; set; }

        /// <summary>
        /// Gets or sets the select group command.
        /// </summary>
        /// <value>
        /// The select group command.
        /// </value>
        public ICommand SelectGroupCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove group command.
        /// </summary>
        /// <value>
        /// The remove group command.
        /// </value>
        public ICommand RemoveGroupCommand { get; set; }        

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAppViewModel"/> class.
        /// </summary>
        public TaskAppViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            this._originalAppWithGroup = new ApplicationWithOverrideVariableGroup();

            InitializeCommands();
        }

        public TaskAppViewModel(TaskApp taskApp)
        {
            if (DesignMode.IsInDesignMode) { return; }

            if (taskApp == null) { throw new ArgumentNullException("taskApp"); }

            this.TaskBase = taskApp;

            InitializeCommands();
            InitializeData(taskApp.AppWithGroup);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskAppViewModel"/> class.
        /// </summary>
        /// <param name="originalAppWithGroup">The app with group.</param>
        public TaskAppViewModel(ApplicationWithOverrideVariableGroup originalAppWithGroup)
        {
            if (DesignMode.IsInDesignMode) { return; }

            InitializeCommands();
            InitializeData(originalAppWithGroup);
        }

        private static void PossiblyHydrateOriginalAppWithGroup(ApplicationWithOverrideVariableGroup originalAppWithGroup)
        {
            if (originalAppWithGroup.Application == null)
            {
                using (var prestoWcf = new PrestoWcf<IApplicationService>())
                {
                    originalAppWithGroup.Application = prestoWcf.Service.GetById(originalAppWithGroup.ApplicationId);
                }
            }

            // If we don't have any custom variable groups, but we have IDs, then get the CVGs by their IDs.
            if ((originalAppWithGroup.CustomVariableGroups == null || originalAppWithGroup.CustomVariableGroups.Count < 1)
                && originalAppWithGroup.CustomVariableGroupIds.Count > 0)
            {
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    foreach (var groupId in originalAppWithGroup.CustomVariableGroupIds)
                    {
                        originalAppWithGroup.CustomVariableGroups.Add(prestoWcf.Service.GetById(groupId));
                    }
                }
            }
        }

        private void InitializeWorkingCopy(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            this.ApplicationWithGroup.Application          = appWithGroup.Application;
            this.ApplicationWithGroup.CustomVariableGroups = appWithGroup.CustomVariableGroups;
            this.ApplicationWithGroup.Enabled              = appWithGroup.Enabled;
        }

        private void InitializeCommands()
        {
            this.OkCommand                = new RelayCommand(Save);
            this.CancelCommand            = new RelayCommand(Cancel);
            this.SelectApplicationCommand = new RelayCommand(SelectApplication);
            this.SelectGroupCommand       = new RelayCommand(SelectGroup);
            this.RemoveGroupCommand       = new RelayCommand(RemoveGroup);

            this.UserCanceled = true;  // default
        }

        private void InitializeData(ApplicationWithOverrideVariableGroup originalAppWithGroup)
        {
            PossiblyHydrateOriginalAppWithGroup(originalAppWithGroup);

            InitializeWorkingCopy(originalAppWithGroup);
            this._originalAppWithGroup = originalAppWithGroup;
        }

        private void Save()
        {
            this.UserCanceled = false;
            UpdateOriginalFromWorkingCopy();
            this.TaskBase = new TaskApp(this._originalAppWithGroup);
            this.Close();
        }

        private void UpdateOriginalFromWorkingCopy()
        {
            this._originalAppWithGroup.Application          = this.ApplicationWithGroup.Application;
            this._originalAppWithGroup.CustomVariableGroups = this.ApplicationWithGroup.CustomVariableGroups;
            this._originalAppWithGroup.Enabled              = this.ApplicationWithGroup.Enabled;
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void SelectApplication()
        {
            ApplicationSelectorViewModel viewModel = new ApplicationSelectorViewModel();
            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);
            if (viewModel.UserCanceled) { return; }

            this.ApplicationWithGroup.Application = viewModel.SelectedApplication;
        }

        private void SelectGroup()
        {
            CustomVariableGroupSelectorViewModel groupViewModel = new CustomVariableGroupSelectorViewModel(true);
            MainWindowViewModel.ViewLoader.ShowDialog(groupViewModel);

            if (groupViewModel.UserCanceled) { return; }
            
            this.ApplicationWithGroup.CustomVariableGroups = groupViewModel.SelectedCustomVariableGroups;
        }

        private void RemoveGroup()
        {
            this.ApplicationWithGroup.CustomVariableGroups = null;
        }    
    }
}
