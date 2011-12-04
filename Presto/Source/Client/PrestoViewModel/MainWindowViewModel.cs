using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel
{
    /// <summary>
    /// View model for main window
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private string _userMessage;

        /// <summary>
        /// Gets or sets the view loader.
        /// </summary>
        /// <value>
        /// The view loader.
        /// </value>
        public static ViewLoader ViewLoader { get; set; }

        /// <summary>
        /// Gets or sets the user message.
        /// </summary>
        /// <value>
        /// The user message.
        /// </value>
        public string UserMessage
        {
            get { return this._userMessage; }

            set
            {
                this._userMessage = value;
                this.NotifyPropertyChanged(() => this.UserMessage);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            // Set a reference to this view model so other view models can access it.
            // One reason for this is so that other view models can set a user message.
            ViewModelUtility.MainWindowViewModel = this;
        }
    }
}
