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
    [TaskTypeAttribute(TaskType.CopyFile)]
    public class TaskCopyFileViewModel : TaskViewModel
    {
        /// <summary>
        /// Gets or sets the task copy file copy. View models will modify this, instead of the real object. We
        /// don't want to alter the real object unless the user saves.
        /// </summary>
        /// <value>
        /// The task copy file copy.
        /// </value>
        public TaskCopyFile TaskCopyFileCopy { get; set; }

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
        /// Initializes a new instance of the <see cref="TaskCopyFileViewModel"/> class.
        /// </summary>
        public TaskCopyFileViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskBase = null;
            this.TaskCopyFileCopy = new TaskCopyFile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCopyFileViewModel"/> class.
        /// </summary>
        /// <param name="taskCopyFile">The task copy file.</param>
        public TaskCopyFileViewModel(TaskCopyFile taskCopyFile)
        {
            if (taskCopyFile == null) { throw new ArgumentNullException("taskCopyFile"); }

            if (DesignMode.IsInDesignMode) { return; }            

            Initialize();

            this.TaskBase          = taskCopyFile;
            this.TaskCopyFileCopy = taskCopyFile.CreateCopyFromThis();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);
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
            if (this.TaskBase == null)
            {
                this.TaskBase = TaskCopyFile.Copy(this.TaskCopyFileCopy, new TaskCopyFile());
                return;
            }

            this.TaskBase = TaskCopyFile.Copy(this.TaskCopyFileCopy, this.TaskBase as TaskCopyFile);
        }
    }
}
