using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for TaskCopyFileView.xaml
    /// </summary>
    [ViewModel(typeof(TaskCopyFileViewModel))]
    public partial class TaskCopyFileView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCopyFileView"/> class.
        /// </summary>
        public TaskCopyFileView()
        {
            InitializeComponent();
        }
    }
}
