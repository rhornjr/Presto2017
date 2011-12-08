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

            this.TaskBase = new TaskXmlModify();

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskXmlModifyViewModel"/> class.
        /// </summary>
        /// <param name="taskXmlModify">The task XML modify.</param>
        public TaskXmlModifyViewModel(TaskXmlModify taskXmlModify)
        {
            if (taskXmlModify == null) { throw new ArgumentNullException("taskXmlModify"); }

            if (DesignMode.IsInDesignMode) { return; }

            this.TaskBase = taskXmlModify;

            Initialize();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(_ => Save());
            this.CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }
    }
}
