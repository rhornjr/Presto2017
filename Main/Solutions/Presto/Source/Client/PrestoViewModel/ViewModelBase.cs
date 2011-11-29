using System.ComponentModel;

namespace PrestoViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null) { return; }

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
