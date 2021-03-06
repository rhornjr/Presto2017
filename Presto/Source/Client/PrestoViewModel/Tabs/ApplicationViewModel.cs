﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Practices.ObjectBuilder2;
using PrestoCommon.Entities;
using PrestoCommon.Entities.LegacyPresto;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;
using Xanico.Core;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationViewModel : ViewModelBase
    {
        private ObservableCollection<Application> _applications;
        private Application _selectedApplication;
        private ObservableCollection<TaskBase> _selectedTasks = new ObservableCollection<TaskBase>();

        public bool ApplicationIsSelected
        {
            get { return ApplicationIsSelectedMethod(); }
        }

        public ICommand AddApplicationCommand { get; private set; }

        public ICommand DeleteApplicationCommand { get; private set; }

        public ICommand RefreshApplicationsCommand { get; private set; }

        public ICommand ForceInstallationCommand { get; private set; }

        public ICommand DeleteForceInstallationCommand { get; private set; }

        public ICommand TaskVersionCheckerCommand { get; private set; }

        public ICommand DeleteTaskVersionCheckerCommand { get; private set; }

        public ICommand AddTaskCommand { get; private set; }

        public ICommand DeleteTaskCommand { get; private set; }

        public ICommand EditTaskCommand { get; private set; }

        public ICommand ExportTasksCommand { get; private set; }

        public ICommand ImportTasksCommand { get; private set; }

        public ICommand MoveTaskUpCommand { get; private set; }

        public ICommand MoveTaskDownCommand { get; private set; }

        public ICommand SaveApplicationCommand { get; private set; }

        public ICommand AddVariableGroupCommand { get; private set; }

        public ICommand RemoveVariableGroupCommand { get; private set; }

        public ObservableCollection<Application> Applications
        {
            get { return this._applications; }

            private set
            {
                this._applications = value;
                this.NotifyPropertyChanged(() => this.Applications);
            }
        }

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
                this.NotifyPropertyChanged(() => this.AllApplicationTasks);
                this.NotifyPropertyChanged(() => this.ApplicationIsSelected);
            }
        }

        public IOrderedEnumerable<TaskBase> AllApplicationTasks
        {
            // Note: This property was created because sorting wasn't working on the grid that showed the tasks.
            //       We have this property so we can return the correctly sorted order.
            get
            {
                if (this.SelectedApplication == null || this.SelectedApplication.Tasks == null) { return null; }
                return this.SelectedApplication.Tasks.OrderBy(task => task.Sequence);
            }
        }

        public ObservableCollection<TaskBase> SelectedTasks
        {
            get { return this._selectedTasks; }

            set
            {
                this._selectedTasks = value;
                NotifyPropertyChanged(() => this.SelectedTasks);
            }
        }

        public bool ShowAllApps { get; set; }

        public CustomVariableGroup SelectedCustomVariableGroup { get; set; }

        public ApplicationViewModel()
        {
            Debug.WriteLine("ApplicationViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
            LoadApplications();

            Debug.WriteLine("ApplicationViewModel constructor end " + DateTime.Now);
        }

        private void Initialize()
        {
            this.AddApplicationCommand      = new RelayCommand(AddApplication);
            this.DeleteApplicationCommand   = new RelayCommand(DeleteApplication, ApplicationIsSelectedMethod);
            this.RefreshApplicationsCommand = new RelayCommand(RefreshApplications);
            this.SaveApplicationCommand     = new RelayCommand(SaveApplication, ApplicationIsSelectedMethod);

            this.ForceInstallationCommand       = new RelayCommand(ForceInstallation, ApplicationIsSelectedMethod);
            this.DeleteForceInstallationCommand = new RelayCommand(DeleteForceInstallation, ApplicationIsSelectedAndForceExists);

            this.TaskVersionCheckerCommand       = new RelayCommand(ShowTaskVersionChecker, ApplicationIsSelectedMethod);
            this.DeleteTaskVersionCheckerCommand = new RelayCommand(DeleteTaskVersionChecker, ApplicationIsSelectedAndVersionCheckerExists);

            this.AddTaskCommand            = new RelayCommand(AddTask, ApplicationIsSelectedMethod);
            this.EditTaskCommand           = new RelayCommand(EditTask, TaskIsSelected);
            this.DeleteTaskCommand         = new RelayCommand(DeleteTask, TaskIsSelected);
            this.ImportTasksCommand        = new RelayCommand(ImportTasks, ApplicationIsSelectedMethod);
            this.ExportTasksCommand        = new RelayCommand(ExportTasks, TaskIsSelected);
            this.MoveTaskUpCommand         = new RelayCommand(MoveRowUp, TaskIsSelected);
            this.MoveTaskDownCommand       = new RelayCommand(MoveRowDown, TaskIsSelected);

            this.AddVariableGroupCommand    = new RelayCommand(AddVariableGroup);
            this.RemoveVariableGroupCommand = new RelayCommand(RemoveVariableGroup, VariableGroupIsSelected);
        }

        private bool ApplicationIsSelectedAndVersionCheckerExists()
        {
            return this.SelectedApplication != null && this.SelectedApplication.TaskVersionChecker != null;
        }

        private void DeleteTaskVersionChecker()
        {
            if (this.SelectedApplication.TaskVersionChecker == null) { return; }  // Nothing to do.            

            this.AddLogMessageToCache(this.SelectedApplication.Id,
                string.Format(CultureInfo.CurrentCulture,
                "Version check removed for app {0}.",
                this.SelectedApplication));

            this.SelectedApplication.TaskVersionChecker = null;
        }

        private void ShowTaskVersionChecker()
        {
            TaskVersionCheckerViewModel viewModel = new TaskVersionCheckerViewModel(this.SelectedApplication.TaskVersionChecker);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplication.TaskVersionChecker = viewModel.TaskBase as TaskVersionChecker;
        }

        // Named this method this way because we have a property of the same name. The RelayCommands need to specify
        // a method, not a property.
        private bool ApplicationIsSelectedMethod()
        {
            return this.SelectedApplication != null;
        }

        private bool ApplicationIsSelectedAndForceExists()
        {
            if (this.ApplicationIsSelected == false) { return false; }

            return this.SelectedApplication.ForceInstallation != null;
        }

        private void RefreshApplications()
        {
            this.LoadApplications();

            ViewModelUtility.MainWindowViewModel.AddUserMessage("Applications refreshed.");
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


            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                prestoWcf.Service.Delete(this.SelectedApplication);
            }

            this.Applications.Remove(this.SelectedApplication);
        }

        private void AddTask()
        {
            TaskViewModel taskViewModel = GetTaskViewModel();

            if (taskViewModel == null) { return; }

            MainWindowViewModel.ViewLoader.ShowDialog(taskViewModel as ViewModelBase);

            if (taskViewModel.UserCanceled) { return; }

            int sequence = 1;  // default

            if (this.SelectedTasks != null && this.SelectedTasks.Count > 0)
            {
                // The user has a row selected, grab that sequence, minus 1, to have the new task before it.
                sequence = this.SelectedTasks[0].Sequence - 1;
            }

            taskViewModel.TaskBase.Sequence = sequence;

            this.SelectedApplication.Tasks.Add(taskViewModel.TaskBase);

            CorrectTaskSequence();

            SaveApplication();

            NotifyPropertyChanged(() => this.AllApplicationTasks);
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
            TaskViewModel taskViewModel = ViewModelUtility.GetViewModel(this.SelectedTasks[0].PrestoTaskType, this.SelectedTasks[0]);

            if (taskViewModel == null) { return; }

            MainWindowViewModel.ViewLoader.ShowDialog(taskViewModel);

            if (taskViewModel.UserCanceled) { return; }

            SaveApplication();
        }

        private bool TaskIsSelected()
        {
            return this.SelectedTasks != null && this.SelectedTasks.Count >0;
        }

        private void DeleteTask()
        {
            string descriptionOfTasks = "multiple tasks";
            if (this.SelectedTasks.Count == 1) { descriptionOfTasks = this.SelectedTasks[0].Description; }
            if (!UserConfirmsDelete(descriptionOfTasks)) { return; }

            try
            {
                // Note: We don't actually delete the task; we just save the application in its new state (without the task).
                
                foreach (var task in this.SelectedTasks)
                {
                    this.SelectedApplication.Tasks.Remove(task);
                }

                CorrectTaskSequence();

                SaveApplication();

                NotifyPropertyChanged(() => this.AllApplicationTasks);
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);
            }
        }

        private void ForceInstallation()
        {
            ForceInstallationViewModel viewModel = new ForceInstallationViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedApplication.ForceInstallation = viewModel.ForceInstallation;

            this.AddLogMessageToCache(this.SelectedApplication.Id, 
                string.Format(CultureInfo.CurrentCulture,
                "{0} selected to be installed in {1} at {2}.",
                this.SelectedApplication,
                this.SelectedApplication.ForceInstallation.ForceInstallEnvironment,
                this.SelectedApplication.ForceInstallation.ForceInstallationTime.ToString()));
        }

        private void DeleteForceInstallation()
        {
            if (this.SelectedApplication.ForceInstallation == null) { return; }  // Nothing to do.            

            this.AddLogMessageToCache(this.SelectedApplication.Id, 
                string.Format(CultureInfo.CurrentCulture,
                "Force installation removed: {0} in {1} at {2}.",
                this.SelectedApplication,
                this.SelectedApplication.ForceInstallation.ForceInstallEnvironment,
                this.SelectedApplication.ForceInstallation.ForceInstallationTime.ToString()));

            this.SelectedApplication.ForceInstallation = null;
        }

        private void ImportTasks()
        {
            // Try to import new tasks. If that fails, automatically try to import legacy tasks. If that fails too, then fail and log.

            string filePathAndName = GetFilePathAndNameFromUser();
            if (string.IsNullOrWhiteSpace(filePathAndName)) { return; }

            Exception exceptionFromGettingNewTasks;

            try
            {
                List<TaskBase> taskBases = TryGetNewTasks(filePathAndName);
                if (taskBases == null) { return; }
                ImportNewTasks(taskBases);
                return;
            }
            catch (Exception ex)
            {
                exceptionFromGettingNewTasks = ex;
            }

            Exception exceptionFromGettingLegacyTasks;

            try
            {
                ObservableCollection<LegacyTaskBase> taskBases = TryGetLegacyTasks(filePathAndName);
                if (taskBases == null) { return; }
                ImportLegacyTasks(taskBases);
                return;
            }
            catch (Exception ex)
            {
                exceptionFromGettingLegacyTasks = ex;
            }

            // If we get here, then we couldn't import either of the two types of tasks (new and legacy).

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                    ViewModelResources.CannotImport));

            Logger.LogException(exceptionFromGettingNewTasks);
            Logger.LogException(exceptionFromGettingLegacyTasks);
        }

        private static List<TaskBase> TryGetNewTasks(string filePathAndName)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                return serializer.Deserialize(fileStream) as List<TaskBase>;
            }
        }

        private void ImportNewTasks(List<TaskBase> taskBases)
        {
            // Start the sequence after the highest existing sequence.
            int sequence = 1;  // default if no tasks exist
            if (this.SelectedApplication.Tasks != null && this.SelectedApplication.Tasks.Count > 0)
            {
                TaskBase highestSequenceTask = this.SelectedApplication.Tasks.OrderByDescending(task => task.Sequence).FirstOrDefault();
                sequence = highestSequenceTask.Sequence + 1;
            }

            foreach (TaskBase task in taskBases.OrderBy(x => x.Sequence))
            {
                task.Id = null;  // new
                task.Sequence = sequence;
                this.SelectedApplication.Tasks.Add(task);
                sequence++;
            }

            SaveApplication();
            NotifyPropertyChanged(() => this.AllApplicationTasks);
        }

        private static ObservableCollection<LegacyTaskBase> TryGetLegacyTasks(string filePathAndName)
        {
            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<LegacyTaskBase>));
                return xmlSerializer.Deserialize(fileStream) as ObservableCollection<LegacyTaskBase>;
            }
        }

        private void ImportLegacyTasks(ObservableCollection<LegacyTaskBase> taskBases)
        {
            // Start the sequence after the highest existing sequence.
            int sequence = 1;  // default if no tasks exist
            if (this.SelectedApplication.Tasks != null && this.SelectedApplication.Tasks.Count > 0)
            {
                TaskBase highestSequenceTask = this.SelectedApplication.Tasks.OrderByDescending(task => task.Sequence).FirstOrDefault();
                sequence = highestSequenceTask.Sequence + 1;
            }

            foreach (LegacyTaskBase legacyTask in taskBases.OrderBy(x => x.Sequence))
            {
                TaskBase task = CreateTaskFromLegacyTask(legacyTask);
                task.Id = null;  // new
                task.Sequence = sequence;
                this.SelectedApplication.Tasks.Add(task);
                sequence++;
            }

            SaveApplication();
            NotifyPropertyChanged(() => this.AllApplicationTasks);
        }

        private void ExportTasks()
        {
            string filePathAndName = SaveFilePathAndNameFromUser(this.SelectedApplication.Name + ".Tasks");

            if (filePathAndName == null) { return; }

            NetDataContractSerializer serializer = new NetDataContractSerializer();

            List<TaskBase> tasksToExport = new List<TaskBase>(this.SelectedTasks);

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Create))
            {
                serializer.WriteObject(fileStream, tasksToExport);
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, this.SelectedApplication.Name + " tasks"));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "PrestoCommon.Misc.Logger.LogWarning(System.String)")]
        private static TaskBase CreateTaskFromLegacyTask(LegacyTaskBase legacyTask)
        {
            Debug.WriteLine(legacyTask.GetType().ToString());

            TaskBase task = null;

            string legacyTaskTypeName = legacyTask.GetType().Name;

            switch (legacyTaskTypeName)
            {
                case "LegacyTaskCopyFile":
                    task = TaskCopyFile.CreateNewFromLegacyTask(legacyTask);
                    break;
                case "LegacyTaskDosCommand":
                    task = TaskDosCommand.CreateNewFromLegacyTask(legacyTask);
                    break;
                case "LegacyTaskXmlModify":
                    task = TaskXmlModify.CreateNewFromLegacyTask(legacyTask);
                    break;
                default:
                    LogUnexpectedLegacyTask(legacyTaskTypeName);
                    break;
            }

            return task;
        }

        private void AddVariableGroup()
        {
            CustomVariableGroupSelectorViewModel viewModel = new CustomVariableGroupSelectorViewModel(null);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            // ToDo: Apps shouldn't reference custom variable groups that are associated with a server.
            // Note: The Presto Task Runner will throw an exception if there are duplicates, but it's
            //       still nice to let the user know right now.
            //if (viewModel.SelectedCustomVariableGroups.Any(group => group.Application != null))
            //{
            //    ViewModelUtility.MainWindowViewModel.UserMessage = ViewModelResources.CannotUseGroup;
            //    return;
            //}

            viewModel.SelectedCustomVariableGroups.ForEach(group => this.SelectedApplication.CustomVariableGroups.Add(group));

            SaveApplication();
        }

        private bool VariableGroupIsSelected()
        {
            return this.SelectedCustomVariableGroup != null;
        }

        private void RemoveVariableGroup()
        {
            this.SelectedApplication.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);

            SaveApplication();
        }

        private static void LogUnexpectedLegacyTask(string legacyTaskTypeName)
        {
            string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.UnexpectedLegacyTask, legacyTaskTypeName);

            ViewModelUtility.MainWindowViewModel.AddUserMessage(message);

            Logger.LogWarning(message);
        }

        private void SaveApplication()
        {
            try
            {
                using (var prestoWcf = new PrestoWcf<IApplicationService>())
                {
                    var savedApplication = prestoWcf.Service.SaveApplication(this.SelectedApplication);
                    this.SelectedApplication = savedApplication;
                    UpdateCacheWithSavedItem(savedApplication);
                }
                this.SaveCachedLogMessages(this.SelectedApplication.Id);
            }
            catch (FaultException ex)
            {
                ViewModelUtility.MainWindowViewModel.AddUserMessage(ex.Message);

                ShowUserMessage(ex.Message, ViewModelResources.ItemNotSavedCaption);

                return;
            }

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedApplication));
        }

        private void UpdateCacheWithSavedItem(Application savedApplication)
        {
            var cachedApp = this.Applications.FirstOrDefault(x => x.Id == savedApplication.Id);

            if (cachedApp == null)
            {
                // We've just added an app and it has a null ID. That's our new one.
                // Note: We can have more than one app with a null ID because a user can add multiple apps without hitting save.
                cachedApp = this.Applications.FirstOrDefault(x => x.Id == null && x.Name == savedApplication.Name);
            }

            if (cachedApp == null)
            {
                this.Applications.Add(savedApplication);
            }
            else
            {
                int index = this.Applications.IndexOf(cachedApp);
                this.Applications[index] = savedApplication;
            }
        }

        private async Task LoadApplications()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var prestoWcf = new PrestoWcf<IApplicationService>())
                    {
                        this.Applications = new ObservableCollection<Application>(prestoWcf.Service.GetAllApplications(this.ShowAllApps));
                    }
                });
            }
            catch (Exception ex)
            {
                CommonUtility.ProcessException(ex);
                ViewModelUtility.MainWindowViewModel.AddUserMessage("Could not load form. Please see log for details.");
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
            int sequenceOfSelectedTask = this.SelectedTasks[0].Sequence;

            if (sequenceOfSelectedTask + moveAmount < 1 || sequenceOfSelectedTask + moveAmount > this.SelectedApplication.Tasks.Count)
            {
                return;  // Can't move an item before the beginning (or after the end) of the list.
            }

            TaskBase taskToSwap = this.SelectedApplication.Tasks.Where(task => task.Sequence == this.SelectedTasks[0].Sequence + moveAmount).FirstOrDefault();

            this.SelectedTasks[0].Sequence += moveAmount;
            taskToSwap.Sequence += moveAmount * -1;

            CorrectTaskSequence();

            NotifyPropertyChanged(() => this.AllApplicationTasks);
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
                }
                properSequence++;
            }
        }
    }
}
