﻿
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;
namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskTypeSelectorViewModel : ViewModelBase
    {
        //private ICommand _okCommand;
        //private ICommand _cancelCommand;

        private TaskType _selectedTaskType;

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
        /// Gets a value indicating whether [user hit cancel].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user hit cancel]; otherwise, <c>false</c>.
        /// </value>
        public bool UserHitCancel { get; private set; }

        /// <summary>
        /// Gets the task types.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<string> TaskTypes
        {
            get
            {
                return Enum.GetNames(typeof (TaskType)).OrderBy(x => x).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the type of the selected task.
        /// </summary>
        /// <value>
        /// The type of the selected task.
        /// </value>
        public TaskType SelectedTaskType
        {
            get { return this._selectedTaskType; }

            set
            {
                this._selectedTaskType = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTypeSelectorViewModel"/> class.
        /// </summary>
        public TaskTypeSelectorViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.OkCommand = new RelayCommand(Ok);
            this.CancelCommand = new RelayCommand(this.Cancel);
        }        

        private void Ok()
        {
            this.Close();
        }

        private void Cancel()
        {
            this.UserHitCancel = true;
            this.Close();
        }
    }
}
