using System;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomVariableViewModel : ViewModelBase
    {
        private CustomVariable _copyOfCustomVariable;

        /// <summary>
        /// Gets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; protected set; }

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
        /// Gets the custom variable.
        /// </summary>
        public CustomVariable CustomVariable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableViewModel"/> class.
        /// </summary>
        public CustomVariableViewModel()
        {
            this.CustomVariable = new CustomVariable();

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableViewModel"/> class.
        /// </summary>
        /// <param name="customVariable">The custom variable.</param>
        public CustomVariableViewModel(CustomVariable customVariable)
        {
            if (customVariable == null) { throw new ArgumentNullException("customVariable"); }

            this.CustomVariable = customVariable;

            this._copyOfCustomVariable = new CustomVariable() { Key = customVariable.Key, Value = customVariable.Value };

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
            // Revert to the original values
            UndoChanges();

            this.UserCanceled = true;
            this.Close();
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
