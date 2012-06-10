using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for ApplicationSelectorView.xaml
    /// </summary>
    [ViewModel(typeof(ApplicationSelectorViewModel))]
    public partial class ApplicationSelectorView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSelectorView"/> class.
        /// </summary>
        public ApplicationSelectorView()
        {
            InitializeComponent();
        }
    }
}
