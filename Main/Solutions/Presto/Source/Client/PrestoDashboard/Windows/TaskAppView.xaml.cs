using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for ApplicationWithGroupView.xaml
    /// </summary>
    [ViewModel(typeof(TaskAppViewModel))]
    public partial class ApplicationWithGroupView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationWithGroupView"/> class.
        /// </summary>
        public ApplicationWithGroupView()
        {
            InitializeComponent();
        }
    }
}
