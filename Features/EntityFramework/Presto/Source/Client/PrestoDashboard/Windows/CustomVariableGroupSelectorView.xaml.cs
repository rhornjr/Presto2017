using System.Windows;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for CustomVariableGroupSelectorView.xaml
    /// </summary>
    [ViewModelAttribute(typeof(CustomVariableGroupSelectorViewModel))]
    public partial class CustomVariableGroupSelectorView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableGroupSelectorView"/> class.
        /// </summary>
        public CustomVariableGroupSelectorView()
        {
            this.InitializeComponent();
        }
    }
}
