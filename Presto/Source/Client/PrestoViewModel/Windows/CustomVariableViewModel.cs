using System;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    public class CustomVariableViewModel : ViewModelBase
    {
        private CustomVariable _copyOfCustomVariable;

        public bool UserCanceled { get; protected set; }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand EncryptCommand { get; set; }

        public CustomVariable CustomVariable { get; private set; }

        public bool ValueIsPlaintext
        {
            get { return !this.CustomVariable.ValueIsEncrypted; }
        }

        public CustomVariableViewModel()
        {
            this.CustomVariable = new CustomVariable();

            Initialize();
        }

        public CustomVariableViewModel(CustomVariable customVariable)
        {
            if (customVariable == null) { throw new ArgumentNullException("customVariable"); }

            this.CustomVariable = customVariable;

            this._copyOfCustomVariable = new CustomVariable() { Key = customVariable.Key, Value = customVariable.Value };

            Initialize();
        }

        private void Initialize()
        {
            this.OkCommand     = new RelayCommand(Save);
            this.CancelCommand = new RelayCommand(Cancel);
            this.EncryptCommand = new RelayCommand(Encrypt);
        }

        private void Save()
        {
            this.Close();
        }

        private void Cancel()
        {
            // Revert to the original values
            UndoChanges();

            this.UserCanceled = true;
            this.Close();
        }

        private void Encrypt()
        {
            this.CustomVariable.Value = AesCrypto.Encrypt(this.CustomVariable.Value);
            this.CustomVariable.ValueIsEncrypted = true;
            this.NotifyPropertyChanged(() => this.ValueIsPlaintext);
        }

        private void UndoChanges()
        {
            // This only applies if we're editing an existing custom variable.
            if (this._copyOfCustomVariable == null) { return; }

            this.CustomVariable.Key   = this._copyOfCustomVariable.Key;
            this.CustomVariable.Value = this._copyOfCustomVariable.Value;
        }
    }
}
