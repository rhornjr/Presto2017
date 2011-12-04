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
        /// Gets or sets the task dos command.
        /// </summary>
        /// <value>
        /// The task dos command.
        /// </value>
        public TaskDosCommand TaskDosCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommandViewModel"/> class.
        /// </summary>
        public TaskDosCommandViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommandViewModel"/> class.
        /// </summary>
        /// <param name="taskDosCommand">The task dos command.</param>
        public TaskDosCommandViewModel(TaskDosCommand taskDosCommand)
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            this.TaskDosCommand = taskDosCommand;
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
            this.TaskDosCommand = null;
            this.Close();
        }
    }
}
