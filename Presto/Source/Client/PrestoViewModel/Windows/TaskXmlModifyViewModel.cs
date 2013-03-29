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
    [TaskTypeAttribute(TaskType.XmlModify)]
    public class TaskXmlModifyViewModel : TaskViewModel
    {
        /// <summary>
        /// Gets or sets the task XML modify copy. View models will modify this, instead of the real object. We
        /// don't want to alter the real object unless the user saves.
        /// </summary>
        public TaskXmlModify TaskXmlModifyCopy { get; set; }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand BrowseFileCommand { get; set; }

        public TaskXmlModifyViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskBase = null;
            this.TaskXmlModifyCopy = new TaskXmlModify();
        }

        public TaskXmlModifyViewModel(TaskXmlModify taskXmlModify)
        {
            if (taskXmlModify == null) { throw new ArgumentNullException("taskXmlModify"); }

            if (DesignMode.IsInDesignMode) { return; }            

            Initialize();

            this.TaskBase           = taskXmlModify;
            this.TaskXmlModifyCopy = taskXmlModify.CreateCopyFromThis();
        }

        private void Initialize()
        {
            this.OkCommand         = new RelayCommand(Save);
            this.CancelCommand     = new RelayCommand(Cancel);
            this.BrowseFileCommand = new RelayCommand(SetXmlDocument);
        }

        private void SetXmlDocument()
        {
            this.TaskXmlModifyCopy.XmlPathAndFileName = GetFilePathAndNameFromUser();
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

        // ToDo: Each task view model has this same algorithm: keep working copy in private variable, same Save()
        //       and Cancel() methods, same algorithm in constructors, and this method below. Considering
        //       refactoring this into one common, generic task view model.
        private void AppyChangesFromCopyToOriginal()
        {
            if (this.TaskBase == null)
            {
                this.TaskBase = TaskXmlModify.Copy(this.TaskXmlModifyCopy, new TaskXmlModify());
                return;
            }

            this.TaskBase = TaskXmlModify.Copy(this.TaskXmlModifyCopy, this.TaskBase as TaskXmlModify);
        }
    }
}
