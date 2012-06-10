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
        /// <value>
        /// The task XML modify copy.
        /// </value>
        public TaskXmlModify TaskXmlModifyCopy { get; set; }

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
        /// Initializes a new instance of the <see cref="TaskXmlModifyViewModel"/> class.
        /// </summary>
        public TaskXmlModifyViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskBase = null;
            this.TaskXmlModifyCopy = new TaskXmlModify();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskXmlModifyViewModel"/> class.
        /// </summary>
        /// <param name="taskXmlModify">The task XML modify.</param>
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
