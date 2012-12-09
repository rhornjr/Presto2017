using System;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    [TaskTypeAttribute(TaskType.DosCommand)]
    public class TaskDosCommandViewModel : TaskViewModel
    {
        /// <summary>
        /// Gets or sets the task dos command copy. View models will modify this, instead of the real object. We
        /// don't want to alter the real object unless the user saves.
        /// </summary>
        /// <value>
        /// The task dos command copy.
        /// </value>
        public TaskDosCommand TaskDosCommandCopy { get; set; }

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
        /// Initializes a new instance of the <see cref="TaskDosCommandViewModel"/> class.
        /// </summary>
        public TaskDosCommandViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskBase           = null;
            this.TaskDosCommandCopy = new TaskDosCommand();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommandViewModel"/> class.
        /// </summary>
        /// <param name="taskDosCommand">The task dos command.</param>
        public TaskDosCommandViewModel(TaskDosCommand taskDosCommand)
        {
            if (taskDosCommand == null) { throw new ArgumentNullException("taskDosCommand"); }

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskBase           = taskDosCommand;
            this.TaskDosCommandCopy = taskDosCommand.CreateCopyFromThis();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            if (!this.TaskDosCommandCopy.IsValid())
            {
                ShowUserMessage(ViewModelResources.TaskInvalidEntryMessage, ViewModelResources.TaskInvalidEntryCaption);
                return;
            }

            AppyChangesFromCopyToOriginal();
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void AppyChangesFromCopyToOriginal()
        {
            if (this.TaskBase == null)
            {
                this.TaskBase = TaskDosCommand.Copy(this.TaskDosCommandCopy, new TaskDosCommand());
                return;
            }

            this.TaskBase = TaskDosCommand.Copy(this.TaskDosCommandCopy, this.TaskBase as TaskDosCommand);
        }
    }
}
