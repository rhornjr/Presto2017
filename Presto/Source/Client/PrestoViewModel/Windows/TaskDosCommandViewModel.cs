using System;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDosCommandViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; private set; }

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
        /// Gets or sets the task dos command original.
        /// </summary>
        /// <value>
        /// The task dos command original.
        /// </value>
        public TaskDosCommand TaskDosCommandOriginal { get; set; }

        /// <summary>
        /// Gets or sets the task dos command copy.
        /// </summary>
        /// <value>
        /// The task dos command copy.
        /// </value>
        public TaskDosCommand TaskDosCommandCopy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommandViewModel"/> class.
        /// </summary>
        public TaskDosCommandViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskDosCommandOriginal = null;
            this.TaskDosCommandCopy     = new TaskDosCommand();
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

            this.TaskDosCommandOriginal = taskDosCommand;
            this.TaskDosCommandCopy     = taskDosCommand.CreateCopyFromThis();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(_ => Save());
            this.CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
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
            if (this.TaskDosCommandOriginal == null)
            {
                this.TaskDosCommandOriginal = TaskDosCommand.Copy(this.TaskDosCommandCopy, new TaskDosCommand());
                return;
            }

            this.TaskDosCommandOriginal = TaskDosCommand.Copy(this.TaskDosCommandCopy, this.TaskDosCommandOriginal);
        }
    }
}
