using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoDashboard.Windows
{
    /// <summary>
    /// Interaction logic for TaskVersionCheckerView.xaml
    /// </summary>
    [ViewModel(typeof(TaskVersionCheckerViewModel))]
    public partial class TaskVersionCheckerView : Window
    {
        public TaskVersionCheckerView()
        {
            InitializeComponent();
        }
    }
}
