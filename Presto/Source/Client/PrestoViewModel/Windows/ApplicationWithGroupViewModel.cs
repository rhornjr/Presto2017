﻿using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationWithGroupViewModel : ViewModelBase
    {
        private ApplicationWithOverrideVariableGroup _applicationWithOverrideVariableGroup;

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

        /// <summary>
        /// Gets or sets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        /// <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; private set; }

        /// <summary>
        /// Gets or sets the application with group.
        /// </summary>
        /// <value>
        /// The application with group.
        /// </value>
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
        /// Initializes a new instance of the <see cref="ApplicationWithGroupViewModel"/> class.
        /// </summary>
        public ApplicationWithGroupViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            this.ApplicationWithGroup = new ApplicationWithOverrideVariableGroup();

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationWithGroupViewModel"/> class.
        /// </summary>
        /// <param name="appWithGroup">The app with group.</param>
        public ApplicationWithGroupViewModel(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (DesignMode.IsInDesignMode) { return; }

            this.ApplicationWithGroup = appWithGroup;

            Initialize();
        }

        private void Initialize()
        {
            this.OkCommand                = new RelayCommand(_ => Save());
            this.CancelCommand            = new RelayCommand(_ => Cancel());
            this.SelectApplicationCommand = new RelayCommand(_ => SelectApplication());
            this.SelectGroupCommand       = new RelayCommand(_ => SelectGroup());
            this.RemoveGroupCommand       = new RelayCommand(_ => RemoveGroup());
        }            

        private void Save()
        {
            this.Close();
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
            CustomVariableGroupSelectorViewModel groupViewModel = new CustomVariableGroupSelectorViewModel();
            MainWindowViewModel.ViewLoader.ShowDialog(groupViewModel);

            if (groupViewModel.UserCanceled) { return; }
            
            this.ApplicationWithGroup.CustomVariableGroup = groupViewModel.SelectedCustomVariableGroup;
        }

        private void RemoveGroup()
        {
            this.ApplicationWithGroup.CustomVariableGroup = null;
        }    
    }
}