using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using PrestoCommon.Entities;
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
        private IObjectContainer _db;

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
            this._db = CommonUtility.GetDatabase();

            this.AddCommand  = new RelayCommand(_ => AddTask());
            this.EditCommand = new RelayCommand(_ => EditTask());
        }

        private static void AddTask()
        {
            MainWindowViewModel.ViewLoader.ShowDialog(new TaskDosCommandViewModel());
        }

        private void EditTask()
        {
            MainWindowViewModel.ViewLoader.ShowDialog(new TaskDosCommandViewModel(this.SelectedTask as TaskDosCommand));

            _db.Store(this.SelectedTask);
            _db.Commit();
        }

        private void LoadApplications()
        {
            try
            {
                IEnumerable<Application> apps = from Application app in this._db
                                                select app;

                this.Applications = new Collection<Application>(apps.ToList());
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
