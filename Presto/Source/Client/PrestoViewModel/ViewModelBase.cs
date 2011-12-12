using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Forms;

namespace PrestoViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Closes this instance.
        /// </summary>
        protected void Close()
        {
            MainWindowViewModel.ViewLoader.CloseView(this);
        }

        /// <summary>
        /// Users the chooses yes.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        protected static bool UserChoosesYes(string message)
        {            
            if (System.Windows.MessageBox.Show(message, ViewModelResources.ConfirmCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Users the confirms delete.
        /// </summary>
        /// <param name="nameOfItemToDelete">The item name to delete.</param>
        /// <returns></returns>
        protected static bool UserConfirmsDelete(string nameOfItemToDelete)
        {
            string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.ConfirmDeleteMessage, nameOfItemToDelete);

            if (System.Windows.MessageBox.Show(message, ViewModelResources.ConfirmDeleteCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the file path and name from user.
        /// </summary>
        /// <returns></returns>
        protected static string GetFilePathAndNameFromUser()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"c:\temp\";

                if (openFileDialog.ShowDialog() == DialogResult.Cancel) { return null; }

                return openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected void NotifyPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (this.PropertyChanged == null) { return; }

            string propertyName = GetPropertyName(expression);

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }        

        private static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;

            return memberExpression.Member.Name;
        }
    }
}
