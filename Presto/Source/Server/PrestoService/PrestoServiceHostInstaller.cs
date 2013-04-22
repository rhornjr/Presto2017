using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace PrestoWcfService
{
    [RunInstaller(true)]
    public partial class PrestoServiceHostInstaller : System.Configuration.Install.Installer
    {
        public PrestoServiceHostInstaller()
        {
            InitializeComponent();
        }
    }
}
