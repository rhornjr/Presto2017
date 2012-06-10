using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for TaskDosCommandView.xaml
    /// </summary>
    [ViewModel(typeof(TaskDosCommandViewModel))]
    public partial class TaskDosCommandView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommandView"/> class.
        /// </summary>
         public TaskDosCommandView()
        {
            InitializeComponent();
        }
    }
}
