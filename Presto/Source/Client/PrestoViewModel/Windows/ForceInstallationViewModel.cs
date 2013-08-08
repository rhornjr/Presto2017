using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class ForceInstallationViewModel : ViewModelBase
    {
        private InstallationEnvironment _selectedDeploymentEnvironment;

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
        /// Gets the force installation now command.
        /// </summary>
        public ICommand ForceInstallationNowCommand { get; private set; }

        /// <summary>
        /// Gets or sets the force installation.
        /// </summary>
        /// <value>
        /// The force installation.
        /// </value>
        public ForceInstallation ForceInstallation { get; set; }

        /// <summary>
        /// Gets the deployment environments that the user is allowed to access.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<InstallationEnvironment> AllowedEnvironments
        {
            get
            {
                return ViewModelUtility.AllowedEnvironments;
            }
        }

        public InstallationEnvironment SelectedDeploymentEnvironment
        {
            get { return this._selectedDeploymentEnvironment; }

            set
            {
                this._selectedDeploymentEnvironment = value;
                NotifyPropertyChanged(() => this.SelectedDeploymentEnvironment);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForceInstallationViewModel"/> class.
        /// </summary>
        public ForceInstallationViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();            
        }

        private void Initialize()
        {
            if (AllowedEnvironments.Count > 0) { this.SelectedDeploymentEnvironment = AllowedEnvironments[0]; }

            this.ForceInstallation = new ForceInstallation();
            this.ForceInstallation.ForceInstallationTime = DateTime.Now;  // default

            this.UserCanceled = true;  // default

            this.OkCommand                   = new RelayCommand(Save, UserCanSave);
            this.CancelCommand               = new RelayCommand(Cancel);
            this.ForceInstallationNowCommand = new RelayCommand(ForceInstallationNow);
        }

        private bool UserCanSave()
        {
            return this.ForceInstallation.ForceInstallationTime != null &&
                this.SelectedDeploymentEnvironment != null;
        }

        private void Save()
        {
            this.UserCanceled = false;
            this.ForceInstallation.ForceInstallEnvironment = this.SelectedDeploymentEnvironment;
            this.Close();
        }

        private void Cancel()
        {
            this.UserCanceled = true;
            this.Close();
        }

        private void ForceInstallationNow()
        {
            this.ForceInstallation.ForceInstallationTime = DateTime.Now;
        }    
    }
}
