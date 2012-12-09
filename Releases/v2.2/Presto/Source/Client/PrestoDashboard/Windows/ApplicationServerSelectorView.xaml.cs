using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for ApplicationServerSelectorView.xaml
    /// </summary>
    [ViewModel(typeof(ApplicationServerSelectorViewModel))]
    public partial class ApplicationServerSelectorView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServerSelectorView"/> class.
        /// </summary>
        public ApplicationServerSelectorView()
        {
            InitializeComponent();
        }
    }
}
