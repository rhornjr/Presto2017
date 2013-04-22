namespace PrestoWcfService
{
    partial class PrestoServiceHostInstaller
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
            this.PrestoServiceHostProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // PrestoServiceHostProcessInstaller
            // 
            this.PrestoServiceHostProcessInstaller.Password = null;
            this.PrestoServiceHostProcessInstaller.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Presto Service Host";
            this.serviceInstaller1.DisplayName = "Presto Service Host";
            this.serviceInstaller1.ServiceName = "PrestoServiceHost";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // PrestoServiceHostInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.PrestoServiceHostProcessInstaller,
            this.serviceInstaller1});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller PrestoServiceHostProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
    }
}