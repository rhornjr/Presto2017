using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    public class RoleSelectorViewModel : ViewModelBase
    {
        public ICommand OkCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public bool UserCanceled { get; private set; }

        private List<PrestoRole> _roles;

        public List<PrestoRole> Roles
        {
            get { return _roles; }

            private set
            {
                _roles = value;
                this.NotifyPropertyChanged(() => this.Roles);
            }
        }

        public PrestoRole SelectedRole { get; set; }

        public RoleSelectorViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
        }

        private void Initialize()
        {
            this.Roles = Enum.GetValues(typeof (PrestoRole)).Cast<PrestoRole>().ToList();

            this.OkCommand    = new RelayCommand(Add);
            this.CancelCommand = new RelayCommand(Cancel);
        }

        private void Add()
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
