using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for RoleSelectorView.xaml
    /// </summary>
    [ViewModel(typeof(RoleSelectorViewModel))]
    public partial class RoleSelectorView : Window
    {
        public RoleSelectorView()
        {
            InitializeComponent();
        }
    }
}
