using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    public class TaskVersionCheckerViewModel : TaskViewModel
    {
        // work with a copy until hitting OK
        public TaskVersionChecker TaskVersionCheckerCopy { get; set; }

        public TaskVersionChecker TaskVersionChecker { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }        

        public TaskVersionCheckerViewModel(TaskVersionChecker taskVersionChecker)
        {
            if (DesignMode.IsInDesignMode) { return; }

            this.TaskVersionChecker = taskVersionChecker;  // store the original, even if null

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
        }

        private void Save()
        {
            this.TaskVersionChecker = this.TaskVersionCheckerCopy.CreateCopyFromThis();
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }
    }
}
