using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Globalization;

namespace SelfUpdatingServiceHost
{
    [RunInstaller(true)]
    public partial class PrestoSelfUpdatingServiceHostInstaller : Installer
    {
        public PrestoSelfUpdatingServiceHostInstaller()
        {
            InitializeComponent();
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            // http://raquila.com/software/configure-app-config-application-settings-during-msi-install/

            // This worked. I didn't have to attach to process manually. In solution explorer, I just right-clicked the
            // installer project and chose Install. These lines below allowed me to debug here.
            //System.Diagnostics.Debugger.Launch();
            //System.Diagnostics.Debugger.Break();

            string targetDirectory = Context.Parameters["targetdir"];

            string prestoTaskRunnerBinaryPath = Context.Parameters["Param1"];            

            string exePath = string.Format(CultureInfo.InvariantCulture, "{0}SelfUpdatingServiceHost.exe", targetDirectory);

            Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);

            config.AppSettings.Settings["SourceBinaryPath"].Value = prestoTaskRunnerBinaryPath;

            config.Save();
        }
    }
}
