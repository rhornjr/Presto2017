using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for ForceInstallationView.xaml
    /// </summary>
    [ViewModel(typeof(ForceInstallationViewModel))]
    public partial class ForceInstallationView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForceInstallationView"/> class.
        /// </summary>
        public ForceInstallationView()
        {
            InitializeComponent();
        }
    }
}
