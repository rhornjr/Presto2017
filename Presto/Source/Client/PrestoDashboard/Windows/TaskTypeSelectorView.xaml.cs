using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for TaskTypeSelectorView.xaml
    /// </summary>
    [ViewModel(typeof(TaskTypeSelectorViewModel))]
    public partial class TaskTypeSelectorView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTypeSelectorView"/> class.
        /// </summary>
        public TaskTypeSelectorView()
        {
            InitializeComponent();
        }
    }
}
