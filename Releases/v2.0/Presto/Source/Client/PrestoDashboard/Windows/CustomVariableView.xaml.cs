using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for CustomVariableView.xaml
    /// </summary>
    [ViewModel(typeof(CustomVariableViewModel))]
    public partial class CustomVariableView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableView"/> class.
        /// </summary>
        public CustomVariableView()
        {
            this.InitializeComponent();
        }
    }
}
