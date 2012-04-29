using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Forms;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Logic;

namespace PrestoViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        private static string lastDialogDirectory;

        // Sometimes we need to record log messages when they happen in real time, but we only want
        // to actually save them if the user hits save. For example, if the user does a force
        // installation for an app, we want to log that event, but we only want to actually save it
        // if the user ends up hitting the Save button for the app.
        // Note: The key is the ID of the entity. The value is the actual message. Since the user
        //       can select different entities, we only want to save the log messages when they
        //       save that particular entity.
        private List<EntityLogMessage> _logMessagesToSaveIfUserHitsSave = new List<EntityLogMessage>();

        /// <summary>
        /// Adds log messages to the cache.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="logMessage"></param>
        protected void AddLogMessageToCache(string entityId, string logMessage)
        {
            this._logMessagesToSaveIfUserHitsSave.Add(new EntityLogMessage(entityId, logMessage));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Really? This really needs an explanation? We, uh, save the cached log messages.
        /// </summary>
        protected void SaveCachedLogMessages(string entityId)
        {
            foreach (EntityLogMessage entityLogMessage in this._logMessagesToSaveIfUserHitsSave)
            {
                if (entityLogMessage.EntityId == entityId) { LogMessageLogic.SaveLogMessage(entityLogMessage.LogMessage); }
            }

            // Remove the processed messages.
            this._logMessagesToSaveIfUserHitsSave.RemoveAll(x => x.EntityId == entityId);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        protected void Close()
        {
            MainWindowViewModel.ViewLoader.CloseView(this);
        }

        /// <summary>
        /// Shows the user message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caption">The caption.</param>
        protected static void ShowUserMessage(string message, string caption)
        {
            System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                openFileDialog.InitialDirectory = lastDialogDirectory;

                if (openFileDialog.ShowDialog() == DialogResult.Cancel) { return null; }

                lastDialogDirectory = Path.GetDirectoryName(openFileDialog.FileName);

                return openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Gets the file path and names from user.
        /// </summary>
        /// <returns></returns>
        protected static string[] GetFilePathAndNamesFromUser()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = lastDialogDirectory;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.Cancel) { return null; }

                lastDialogDirectory = Path.GetDirectoryName(openFileDialog.FileName);

                return openFileDialog.FileNames;
            }
        }

        /// <summary>
        /// Saves the file path and name from user.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        protected static string SaveFilePathAndNameFromUser(string fileName)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = fileName;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.InitialDirectory = lastDialogDirectory;

                if (saveFileDialog.ShowDialog() == DialogResult.Cancel) { return null; }

                lastDialogDirectory = Path.GetDirectoryName(saveFileDialog.FileName);

                return saveFileDialog.FileName;
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
