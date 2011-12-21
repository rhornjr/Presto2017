using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
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
        /// Gets the force installation command.
        /// </summary>
        public ICommand ForceInstallationCommand { get; private set; }

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
        /// Gets the move task up command.
        /// </summary>
        public ICommand MoveTaskUpCommand { get; private set; }

        /// <summary>
        /// Gets the move task down command.
        /// </summary>
        public ICommand MoveTaskDownCommand { get; private set; }

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
            get
            {                
                return this._selectedApplication;                
            }

            set
            {
                this._selectedApplication = value;
                this.NotifyPropertyChanged(() => this.SelectedApplication);
                this.NotifyPropertyChanged(() => this.SelectedApplicationTasks);
            }
        }

        /// <summary>
        /// Gets the selected application tasks.
        /// </summary>
        public IOrderedEnumerable<TaskBase> SelectedApplicationTasks
        {
            // Note: This property was created because sorting wasn't working on the grid that showed the tasks.
            //       We have this property so we can return the correctly sorted order.
            get
            {
                if (this.SelectedApplication == null || this.SelectedApplication.Tasks == null) { return null; }
                return this.SelectedApplication.Tasks.OrderBy(task => task.Sequence);
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

            this.ForceInstallationCommand = new RelayCommand(_ => ForceInstallation(), _ => ApplicationIsSelected());

            this.AddTaskCommand      = new RelayCommand(_ => AddTask(), _ => ApplicationIsSelected());
            this.EditTaskCommand     = new RelayCommand(_ => EditTask(), _ => TaskIsSelected());
            this.DeleteTaskCommand   = new RelayCommand(_ => DeleteTask(), _ => TaskIsSelected());
            this.ImportTasksCommand  = new RelayCommand(_ => ImportTasks(), _ => ApplicationIsSelected());
            this.MoveTaskUpCommand   = new RelayCommand(_ => MoveRowUp(), _ => TaskIsSelected());
            this.MoveTaskDownCommand = new RelayCommand(_ => MoveRowDown(), _ => TaskIsSelected());
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

            int sequence = 1;  // default

            if (this.SelectedTask != null)
            {
                // The user has a row selected, grab that sequence, minus 1, to have the new task before it.
                sequence = this.SelectedTask.Sequence - 1;
            }

            taskViewModel.TaskBase.Sequence = sequence;

            this.SelectedApplication.Tasks.Add(taskViewModel.TaskBase);

            CorrectTaskSequence();

            NotifyPropertyChanged(() => this.SelectedApplicationTasks);

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

            try
            {
                LogicBase.Delete<TaskBase>(this.SelectedTask);

                this.SelectedApplication.Tasks.Remove(this.SelectedTask);

                CorrectTaskSequence();

                NotifyPropertyChanged(() => this.SelectedApplicationTasks);

                LogicBase.Save<Application>(this.SelectedApplication);
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = ex.Message;
            }
        }

        private void ForceInstallation()
        {
            ForceInstallationViewModel viewModel = new ForceInstallationViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            LogUtility.LogInformation(string.Format(CultureInfo.CurrentCulture,
                "{0} selected to be installed by {1}.",
                this.SelectedApplication,
                WindowsIdentity.GetCurrent().Name));

            this.SelectedApplication.ForceInstallation = viewModel.ForceInstallation;
        }   

        private void ImportTasks()
        {
            ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase> taskBases = new ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>();

            string filePathAndName = GetFilePathAndNameFromUser();

            if (string.IsNullOrWhiteSpace(filePathAndName)) { return; }

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>));
                taskBases = xmlSerializer.Deserialize(fileStream) as ObservableCollection<PrestoCommon.Entities.LegacyPresto.TaskBase>;
            }

            int sequence = 1;
            foreach (PrestoCommon.Entities.LegacyPresto.TaskBase legacyTask in taskBases)
            {
                TaskBase task = CreateTaskFromLegacyTask(legacyTask);
                task.Sequence = sequence;
                this.SelectedApplication.Tasks.Add(task);
                sequence++;
            }            

            ApplicationLogic.Save(this.SelectedApplication);
            NotifyPropertyChanged(() => this.SelectedApplicationTasks);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "PrestoCommon.Misc.LogUtility.LogWarning(System.String)")]
        private static TaskBase CreateTaskFromLegacyTask(PrestoCommon.Entities.LegacyPresto.TaskBase legacyTask)
        {
            Debug.WriteLine(legacyTask.GetType().ToString());

            TaskBase task = null;

            string legacyTaskTypeName = legacyTask.GetType().Name;

            switch (legacyTaskTypeName)
            {
                case "TaskCopyFile":
                    task = TaskCopyFile.CreateNewFromLegacyTask(legacyTask);
                    break;
                case "TaskDosCommand":
                    task = TaskDosCommand.CreateNewFromLegacyTask(legacyTask);
                    break;
                case "TaskXmlModify":
                    task = TaskXmlModify.CreateNewFromLegacyTask(legacyTask);
                    break;
                default:
                    LogUnexpectedLegacyTask(legacyTaskTypeName);
                    break;
            }

            return task;
        }

        private static void LogUnexpectedLegacyTask(string legacyTaskTypeName)
        {
            string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.UnexpectedLegacyTask, legacyTaskTypeName);

            ViewModelUtility.MainWindowViewModel.UserMessage = message;

            LogUtility.LogWarning(message);
        }

        private void SaveApplication()
        {
            LogicBase.Save<Application>(this.SelectedApplication);

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedApplication);
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

        private void MoveRowUp()
        {
            MoveRow(-1);
        }

        private void MoveRowDown()
        {
            MoveRow(+1);
        }

        private void MoveRow(int moveAmount)
        {
            int sequenceOfSelectedTask = this.SelectedTask.Sequence;

            if (sequenceOfSelectedTask + moveAmount < 1 || sequenceOfSelectedTask + moveAmount > this.SelectedApplication.Tasks.Count - 1)
            {
                return;  // Can't move an item before the beginning (or after the end) of the list.
            }

            TaskBase taskToSwap = this.SelectedApplication.Tasks.Where(task => task.Sequence == this.SelectedTask.Sequence + moveAmount).FirstOrDefault();

            this.SelectedTask.Sequence += moveAmount;
            taskToSwap.Sequence += moveAmount * -1;

            CorrectTaskSequence();

            NotifyPropertyChanged(() => this.SelectedApplicationTasks);

            // After doing all of this, select the task again so it stays highlighted for the user.
            //this.SelectedTasks.Clear();
            //this.SelectedTasks.Add(this.Tasks[sequenceOfSelectedTask + moveAmount]);
        }

        private void CorrectTaskSequence()
        {
            // Loop through the tasks. If the sequence is incorrect, make it correct.            
            int properSequence = 1;
            foreach (TaskBase taskBase in this.SelectedApplication.Tasks.OrderBy(task => task.Sequence))
            {
                if (taskBase.Sequence != properSequence)
                {
                    taskBase.Sequence = properSequence;
                    LogicBase.Save<TaskBase>(taskBase);
                }
                properSequence++;
            }
        }
    }
}
