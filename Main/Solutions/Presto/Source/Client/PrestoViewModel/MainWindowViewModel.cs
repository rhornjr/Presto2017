using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Threading;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private static readonly object _locker = new object();
        private int _maxMessagesToDisplay = 5;
        private ObservableCollection<string> _userMessages = new ObservableCollection<string>();

        public static ViewLoader ViewLoader { get; set; }

        public ObservableCollection<string> UserMessages
        {
            get { return this._userMessages; }
        }

        public MainWindowViewModel()
        {
            // Set a reference to this view model so other view models can access it.
            // One reason for this is so that other view models can set a user message.
            ViewModelUtility.MainWindowViewModel = this;
        }

        public void AddUserMessage(string message)
        {
            // Note: We need to use Dispatcher.BeginInvoke() because this method is called by the UI
            //       thread and events (not the UI thread).
            lock (_locker)
            {
                Action action = () =>
                {
                    this._userMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "{0}: {1}",
                        DateTime.Now.ToString(CultureInfo.CurrentCulture),
                        message));

                    // If we've exceeded our maximum number of messages to display, remove the first item.
                    if (this._userMessages.Count > _maxMessagesToDisplay) { this._userMessages.RemoveAt(0); }
                };

                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
        }
    }
}
