using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public class ForceInstallationViewModel : ViewModelBase
    {
        private List<DeploymentEnvironment> _deploymentEnvironments;
        private DeploymentEnvironment _selectedDeploymentEnvironment;

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
        /// Gets the deployment environments.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<DeploymentEnvironment> DeploymentEnvironments
        {
            get
            {
                if (this._deploymentEnvironments == null)
                {
                    this._deploymentEnvironments = Enum.GetValues(typeof(DeploymentEnvironment)).Cast<DeploymentEnvironment>().ToList();
                    this._deploymentEnvironments.Remove(DeploymentEnvironment.Unknown);  // Don't let Unknown be an option.
                    if (this.SelectedDeploymentEnvironment == DeploymentEnvironment.Unknown) { this.SelectedDeploymentEnvironment = this._deploymentEnvironments[0]; } // Set selected to first as default
                }
                return this._deploymentEnvironments;
            }
        }

        /// <summary>
        /// Gets or sets the selected deployment environment.
        /// </summary>
        /// <value>
        /// The selected deployment environment.
        /// </value>
        public DeploymentEnvironment SelectedDeploymentEnvironment
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
        public ForceInstallationViewModel(ForceInstallation forceInstallation)
        {
            if (DesignMode.IsInDesignMode) { return; }

            // Set the default environment to whatever was used last.
            if (forceInstallation != null) { this.SelectedDeploymentEnvironment = forceInstallation.ForceInstallationEnvironment; }

            Initialize();            
        }

        private void Initialize()
        {
            this.ForceInstallation = new ForceInstallation();

            this.OkCommand                   = new RelayCommand(Save, ForceInstallationTimeIsValid);
            this.CancelCommand               = new RelayCommand(Cancel);
            this.ForceInstallationNowCommand = new RelayCommand(ForceInstallationNow);
        }

        private bool ForceInstallationTimeIsValid()
        {
            return this.ForceInstallation.ForceInstallationTime != null;
        }

        private void Save()
        {
            this.ForceInstallation.ForceInstallationEnvironment = this.SelectedDeploymentEnvironment;
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
