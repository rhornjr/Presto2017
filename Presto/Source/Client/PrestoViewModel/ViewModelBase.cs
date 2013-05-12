using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Practices.Unity;
using PrestoCommon.Interfaces;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Factories.OpenFileDialog;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;

namespace PrestoViewModel
{
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

        protected void AddLogMessageToCache(string entityId, string logMessage)
        {
            this._logMessagesToSaveIfUserHitsSave.Add(new EntityLogMessage(entityId, logMessage));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SaveCachedLogMessages(string entityId)
        {
            using (var prestoWcf = new PrestoWcf<IBaseService>())
            {
                foreach (EntityLogMessage entityLogMessage in this._logMessagesToSaveIfUserHitsSave)
                {
                    if (entityLogMessage.EntityId == entityId)
                    {
                        prestoWcf.Service.SaveLogMessage(entityLogMessage.LogMessage);
                    }
                }
            }

            // Remove the processed messages.
            this._logMessagesToSaveIfUserHitsSave.RemoveAll(x => x.EntityId == entityId);
        }

        protected void Close()
        {
            MainWindowViewModel.ViewLoader.CloseView(this);
        }

        protected static void ShowUserMessage(string message, string caption)
        {
            System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        protected static bool UserChoosesYes(string message)
        {            
            if (System.Windows.MessageBox.Show(message, ViewModelResources.ConfirmCaption,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return false;
            }

            return true;
        }

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

        protected static string GetFilePathAndNameFromUser()
        {
            using (IOpenFileDialogService dialogService = CommonUtility.Container.Resolve<IOpenFileDialogService>())
            {
                dialogService.InitialDirectory = lastDialogDirectory;

                if (dialogService.ShowDialog() == DialogResult.Cancel) { return null; }

                lastDialogDirectory = Path.GetDirectoryName(dialogService.FileName);

                return dialogService.FileName;
            }
        }

        protected static string[] GetFilePathAndNamesFromUser()
        {
            using (IOpenFileDialogService dialogService = CommonUtility.Container.Resolve<IOpenFileDialogService>())
            {
                dialogService.InitialDirectory = lastDialogDirectory;
                dialogService.Multiselect = true;

                if (dialogService.ShowDialog() == DialogResult.Cancel) { return null; }

                lastDialogDirectory = Path.GetDirectoryName(dialogService.FileName);

                return dialogService.FileNames;
            }
        }

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
