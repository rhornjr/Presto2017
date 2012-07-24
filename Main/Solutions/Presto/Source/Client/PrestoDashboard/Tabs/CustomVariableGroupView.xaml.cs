using System.Windows.Controls;
using System.Windows.Data;
using PrestoCommon.Entities;

namespace PrestoDashboard.Tabs
{
    /// <summary>
    /// Interaction logic for GlobalVariableView.xaml
    /// </summary>
    public partial class GlobalVariableView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalVariableView"/> class.
        /// </summary>
        public GlobalVariableView()
        {
            InitializeComponent();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            CustomVariableGroup customVariableGroup = e.Item as CustomVariableGroup;

            if (customVariableGroup == null) { return; }

            if ((bool)chkShowDisabled.IsChecked)
            {
                // Show everything
                e.Accepted = true;
                return;
            }

            // We are not showing disabled items, so set disabled items e.Accepted to false.
            if (customVariableGroup.Disabled == true)
            {
                e.Accepted = false;
                return;
            }

            e.Accepted = true;
        }
    }
}
