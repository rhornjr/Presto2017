using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;


namespace PrestoDashboard
{
    /// <summary>
    /// 
    /// </summary>
    [RunInstaller(true)]
    public partial class PrestoDashboardInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// 
        /// </summary>
        public PrestoDashboardInstaller()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateSaver"></param>
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            // http://raquila.com/software/configure-app-config-application-settings-during-msi-install/

            // This worked. I didn't have to attach to process manually. In solution explorer, I just right-clicked the
            // installer project and chose Install. These lines below allowed me to debug here.
            //System.Diagnostics.Debugger.Launch();
            //System.Diagnostics.Debugger.Break();

            string targetDirectory = Context.Parameters["targetdir"];

            string connectionString = Context.Parameters["Param1"];

            string exePath = string.Format(CultureInfo.InvariantCulture, "{0}PrestoDashboard.exe", targetDirectory);

            Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

            config.ConnectionStrings.ConnectionStrings["RavenDb"].ConnectionString = connectionString;

            config.Save();
        }
    }
}
