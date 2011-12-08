using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for TaskXmlModifyView.xaml
    /// </summary>
    [ViewModelAttribute(typeof(TaskXmlModifyViewModel))]
    public partial class TaskXmlModifyView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskXmlModifyView"/> class.
        /// </summary>
        public TaskXmlModifyView()
        {
            InitializeComponent();
        }
    }
}
