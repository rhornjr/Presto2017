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
                }
                return this._deploymentEnvironments;
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
            this.ForceInstallation = new ForceInstallation();

            this.OkCommand                   = new RelayCommand(_ => Save());
            this.CancelCommand               = new RelayCommand(_ => Cancel());
            this.ForceInstallationNowCommand = new RelayCommand(_ => ForceInstallationNow());
        }

        private void Save()
        {
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
