using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    [TaskTypeAttribute(TaskType.VersionChecker)]
    public class TaskVersionCheckerViewModel : TaskViewModel
    {
        // work with a copy until hitting OK
        public TaskVersionChecker TaskVersionCheckerCopy { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public TaskVersionCheckerViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            this.TaskVersionCheckerCopy = new TaskVersionChecker();

            Initialize();
        }

        public TaskVersionCheckerViewModel(TaskVersionChecker taskVersionChecker)
        {
            if (DesignMode.IsInDesignMode) { return; }

            this.TaskBase = taskVersionChecker;  // store the original, even if null

            this.TaskVersionCheckerCopy = new TaskVersionChecker();
            if (taskVersionChecker != null)
            {
                this.TaskVersionCheckerCopy = taskVersionChecker.CreateCopyFromThis();
            }

            Initialize();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);

            this.UserCanceled = true;  // default
        }

        private void Save()
        {
            this.UserCanceled = false;
            ApplyChangesFromCopyToOriginal();
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void ApplyChangesFromCopyToOriginal()
        {
            if (this.TaskBase == null)
            {
                this.TaskBase = TaskVersionChecker.Copy(this.TaskVersionCheckerCopy, new TaskVersionChecker());
                return;
            }

            this.TaskBase = TaskVersionChecker.Copy(this.TaskVersionCheckerCopy, this.TaskBase as TaskVersionChecker);
        }
    }
}
