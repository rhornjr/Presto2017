namespace SelfUpdatingServiceHost
{
    partial class PrestoSelfUpdatingServiceHostInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PrestoSelfUpdatingServiceHostProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.ServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // PrestoSelfUpdatingServiceHostProcessInstaller
            // 
            this.PrestoSelfUpdatingServiceHostProcessInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.PrestoSelfUpdatingServiceHostProcessInstaller.Password = null;
            this.PrestoSelfUpdatingServiceHostProcessInstaller.Username = null;
            // 
            // ServiceInstaller
            // 
            this.ServiceInstaller.Description = "Presto Self Updating Service Host";
            this.ServiceInstaller.DisplayName = "Presto Self Updating Service Host";
            this.ServiceInstaller.ServiceName = "Presto Self Updating Service Host";
            this.ServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // SelfUpdatingServiceInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.PrestoSelfUpdatingServiceHostProcessInstaller,
            this.ServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller PrestoSelfUpdatingServiceHostProcessInstaller;
        private System.ServiceProcess.ServiceInstaller ServiceInstaller;
    }
}