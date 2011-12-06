﻿using System;
using System.Collections.ObjectModel;
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
    public class ApplicationViewModel : ViewModelBase
    {
        private Collection<Application> _applications;
        private Application _selectedApplication;
        private TaskBase _selectedTask;               

        /// <summary>
        /// Gets the add command.
        /// </summary>
        public ICommand AddCommand { get; private set; }

        /// <summary>
        /// Gets or sets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        public ICommand EditCommand { get; private set; }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public Collection<Application> Applications
        {
            get { return this._applications; }

            private set
            {
                this._applications = value;
                this.NotifyPropertyChanged(() => this.Applications);
            }
        }

        /// <summary>
        /// Gets or sets the selected application.
        /// </summary>
        /// <value>
        /// The selected application.
        /// </value>
        public Application SelectedApplication
        {
            get { return this._selectedApplication; }

            set
            {
                this._selectedApplication = value;
                this.NotifyPropertyChanged(() => this.SelectedApplication);
            }
        }

        /// <summary>
        /// Gets or sets the selected task.
        /// </summary>
        /// <value>
        /// The selected task.
        /// </value>
        public TaskBase SelectedTask
        {
            get { return this._selectedTask; }

            set
            {
                this._selectedTask = value;
                NotifyPropertyChanged(() => this.SelectedTask);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationViewModel"/> class.
        /// </summary>
        public ApplicationViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadApplications();
        }

        private void Initialize()
        {
            this.AddCommand  = new RelayCommand(_ => AddTask());
            this.EditCommand = new RelayCommand(_ => EditTask());
        }

        private static void AddTask()
        {
            TaskTypeSelectorViewModel viewModel = new TaskTypeSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserHitCancel == true) { return; }

            Type type = ViewModelUtility.GetViewModel(viewModel.SelectedTaskType);

            ViewModelBase viewModelBase = Activator.CreateInstance(type) as ViewModelBase;

            MainWindowViewModel.ViewLoader.ShowDialog(viewModelBase);

            //TaskDosCommandViewModel viewModel = new TaskDosCommandViewModel();

            //MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            //if (viewModel.UserCanceled) { return; }

            //this.SelectedApplication.Tasks.Add(viewModel.TaskDosCommandOriginal);

            //ApplicationLogic.Save(this.SelectedApplication);
        }

        private void EditTask()
        {
            TaskDosCommandViewModel viewModel = new TaskDosCommandViewModel(this.SelectedTask as TaskDosCommand);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            TaskDosCommandLogic.Save(viewModel.TaskDosCommandOriginal);
        }

        private void LoadApplications()
        {            
            try
            {
                this.Applications = new Collection<Application>(ApplicationLogic.GetAll().ToList());
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
