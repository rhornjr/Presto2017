using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace PrestoDashboard
{
    [RunInstaller(true)]
    public partial class PrestoDashboardInstaller : System.Configuration.Install.Installer
    {
        public PrestoDashboardInstaller()
        {
            InitializeComponent();
        }
    }
}
