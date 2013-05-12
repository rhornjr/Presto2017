using System.Windows.Controls;

namespace PrestoDashboard.Tabs
{
    /// <summary>
    /// Interaction logic for ApplicationServerView.xaml
    /// </summary>
    public partial class ApplicationServerView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationServerView"/> class.
        /// </summary>
        public ApplicationServerView()
        {
            InitializeComponent();
        }

        //private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        //{
        //    ((ApplicationServerViewModel)DataContext).SelectedApplicationServer = e.NewValue as ApplicationServer;
        //}
    }
}
