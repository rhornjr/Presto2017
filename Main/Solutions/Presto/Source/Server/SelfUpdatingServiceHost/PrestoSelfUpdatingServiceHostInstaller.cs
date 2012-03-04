using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace SelfUpdatingServiceHost
{
    [RunInstaller(true)]
    public partial class PrestoSelfUpdatingServiceHostInstaller : System.Configuration.Install.Installer
    {
        public PrestoSelfUpdatingServiceHostInstaller()
        {
            InitializeComponent();
        }
    }
}
