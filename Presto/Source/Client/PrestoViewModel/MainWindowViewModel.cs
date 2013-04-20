using System;
using System.Collections.ObjectModel;
using System.Globalization;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
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
            // ToDo: Implement RavenDB push notifications:
            //       http://ravendb.net/docs/2.0/client-api/changes-api
            // Do this so we can display new installation summary and log notices.

            this._userMessages.Add(string.Format(CultureInfo.CurrentCulture,
                    "{0}: {1}",
                    DateTime.Now.ToString(),
                    message));

            // If we've exceeded our maximum number of messages to display, remove the first item.
            if (this._userMessages.Count > _maxMessagesToDisplay) { this._userMessages.RemoveAt(0); }

            this.NotifyPropertyChanged(() => this.UserMessages);
        }
    }
}
