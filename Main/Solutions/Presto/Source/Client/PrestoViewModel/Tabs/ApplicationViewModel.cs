using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Input;
using System.Xml.Serialization;
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
        private ObservableCollection<Application> _applications;
        private Application _selectedApplication;
        private TaskBase _selectedTask;

        /// <summary>
        /// Gets the add application command.
        /// </summary>
        public ICommand AddApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the delete application command.
        /// </summary>
        public ICommand DeleteApplicationCommand { get; private set; }

        /// <summary>
        /// Gets the force installation now command.
        /// </summary>
        public ICommand ForceInstallationNowCommand { get; private set; }

        /// <summary>
        /// Gets the add command.
        /// </summary>
        public ICommand AddTaskCommand { get; private set; }

        /// <summary>
        /// Gets the delete task command.
        /// </summary>
        public ICommand DeleteTaskCommand { get; private set; }

        /// <summary>
        /// Gets or sets the edit command.
        /// </summary>
        /// <value>
        /// The edit command.
        /// </value>
        public ICommand EditTaskCommand { get; private set; }

        /// <summary>
        /// Gets the import tasks command.
        /// </summary>
        public ICommand ImportTasksCommand { get; private set; }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        public ICommand SaveApplicationCommand { get; private set; }

        /// <summary>
        /// Gets or sets the applications.
        /// </summary>
        /// <value>
        /// The applications.
        /// </value>
        public ObservableCollection<Application> Applications
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
            this.AddApplicationCommand    = new RelayCommand(_ => AddApplication());
            this.DeleteApplicationCommand = new RelayCommand(_ => DeleteApplication(), _ => ApplicationIsSelected());
            this.SaveApplicationCommand   = new RelayCommand(_ => SaveApplication(), _ => ApplicationIsSelected());

            this.AddTaskCommand     = new RelayCommand(_ => AddTask(), _ => ApplicationIsSelected());
            this.EditTaskCommand    = new RelayCommand(_ => EditTask(), _ => TaskIsSelected());
            this.DeleteTaskCommand  = new RelayCommand(_ => DeleteTask(), _ => TaskIsSelected());
            this.ImportTasksCommand = new RelayCommand(_ => ImportTasks(), _ => ApplicationIsSelected());

            this.ForceInstallationNowCommand = new RelayCommand(_ => ForceInstallationNow());
        }             

        private void ForceInstallationNow()
        {
            this.SelectedApplication.ForceInstallationTime = DateTime.Now;
        }             

        private void AddApplication()
        {
            string newAppName = "New App - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            this.Applications.Add(new Application() { Name = newAppName });

            this.SelectedApplication = this.Applications.Where(app => app.Name == newAppName).FirstOrDefault();
        }

        private void DeleteApplication()
        {
            if (!UserConfirmsDelete(this.SelectedApplication.Name)) { return; }

            LogicBase.Delete<Application>(this.SelectedApplication);
            
            this.Applications.Remove(this.SelectedApplication);
        }        

        private void AddTask()
        {
            TaskViewModel taskViewModel = GetTaskViewModel();

            if (taskViewModel == null) { return; }

            MainWindowViewModel.ViewLoader.ShowDialog(taskViewModel as ViewModelBase);

            if (taskViewModel.UserCanceled) { return; }

            this.SelectedApplication.Tasks.Add(taskViewModel.TaskBase);

            ApplicationLogic.Save(this.SelectedApplication);
        }

        private static TaskViewModel GetTaskViewModel()
        {
            TaskTypeSelectorViewModel selectorViewModel = new TaskTypeSelectorViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(selectorViewModel);

            if (selectorViewModel.UserHitCancel == true) { return null; }            

            TaskViewModel taskViewModel = ViewModelUtility.GetViewModel(selectorViewModel.SelectedTaskType);

            return taskViewModel;
        }

        private void EditTask()
        {
            TaskViewModel taskViewModel = ViewModelUtility.GetViewModel(this.SelectedTask.PrestoTaskType, this.SelectedTask);

            if (taskViewModel == null) { return; }

            MainWindowViewModel.ViewLoader.ShowDialog(taskViewModel);

            if (taskViewModel.UserCanceled) { return; }

            LogicBase.Save(taskViewModel.TaskBase);
        }

        private bool ApplicationIsSelected()
        {
            return this.SelectedApplication != null;
        }

        private bool TaskIsSelected()
        {
            return this.SelectedTask != null;
        }   

        private void DeleteTask()
        {
            if (!UserConfirmsDelete(this.SelectedTask.Description)) { return; }

            LogicBase.Delete<TaskBase>(this.SelectedTask);

            this.SelectedApplication.Tasks.Remove(this.SelectedTask);

            LogicBase.Save<Application>(this.SelectedApplication);
        }

        private static void ImportTasks()
        {
            ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase> taskBases = new ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>();

            using (FileStream fileStream = new FileStream(@"c:\temp\Tasks.presto", FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>));
                taskBases = xmlSerializer.Deserialize(fileStream) as ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>;
            }

            foreach (PrestoCommon.Entities.LegacyPresto.TaskBase legacyTaskBase in taskBases)
            {
                Debug.WriteLine(legacyTaskBase.Description);
            }
        }   

        private void SaveApplication()
        {
            LogicBase.Save<Application>(this.SelectedApplication);
        }

        private void LoadApplications()
        {            
            try
            {
                this.Applications = new ObservableCollection<Application>(ApplicationLogic.GetAll().ToList());
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
