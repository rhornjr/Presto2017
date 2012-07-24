﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using PrestoCommon.Entities;
using PrestoCommon.Logic;
using PrestoCommon.Misc;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;
using Raven.Abstractions.Exceptions;

namespace PrestoViewModel.Tabs
{
    public class CustomVariableGroupViewModel : ViewModelBase
    {
        private ObservableCollection<CustomVariableGroup> _customVariableGroups;
        private CustomVariableGroup _selectedCustomVariableGroup;
        private bool _showDisabled;

        public bool CustomVariableGroupIsSelected
        {
            get { return this.CustomVariableGroupIsSelectedMethod(); }
        }

        public ICommand AddVariableGroupCommand { get; private set; }

        public ICommand DeleteVariableGroupCommand { get; private set; }

        public ICommand ImportVariableGroupCommand { get; private set; }        

        public ICommand ExportVariableGroupCommand { get; private set; }

        public ICommand RefreshVariableGroupCommand { get; private set; }

        public ICommand SaveVariableGroupCommand { get; private set; }

        public ICommand AddVariableCommand { get; private set; }

        public ICommand EditVariableCommand { get; private set; }

        public ICommand DeleteVariableCommand { get; private set; }

        public bool ShowDisabled
        {
            get { return this._showDisabled; }

            set
            {
                this._showDisabled = value;
                NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        public ObservableCollection<CustomVariableGroup> CustomVariableGroups
        {
            get
            {
                return this._customVariableGroups;
            }

            private set
            {
                this._customVariableGroups = value;
                NotifyPropertyChanged(() => this.CustomVariableGroups);
            }
        }

        public CustomVariableGroup SelectedCustomVariableGroup
        {
            get { return this._selectedCustomVariableGroup; }

            set
            {
                this._selectedCustomVariableGroup = value;
                NotifyPropertyChanged(() => this.SelectedCustomVariableGroup);
                NotifyPropertyChanged(() => this.CustomVariableGroupIsSelected);
            }
        }

        /// <summary>
        /// Gets or sets the selected custom variable.
        /// </summary>
        /// <value>
        /// The selected custom variable.
        /// </value>
        public CustomVariable SelectedCustomVariable { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomVariableGroupViewModel"/> class.
        /// </summary>
        public CustomVariableGroupViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            LoadCustomVariableGroups();
        }

        private void Initialize()
        {
            this.AddVariableGroupCommand     = new RelayCommand(AddVariableGroup);
            this.DeleteVariableGroupCommand  = new RelayCommand(DeleteVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.SaveVariableGroupCommand    = new RelayCommand(_ => SaveVariableGroup(null), CustomVariableGroupIsSelectedMethod);
            this.ExportVariableGroupCommand  = new RelayCommand(ExportVariableGroup, CustomVariableGroupIsSelectedMethod);
            this.ImportVariableGroupCommand  = new RelayCommand(ImportVariableGroup);
            this.RefreshVariableGroupCommand = new RelayCommand(RefreshVariableGroup);

            this.AddVariableCommand    = new RelayCommand(AddVariable);
            this.EditVariableCommand   = new RelayCommand(EditVariable, CustomVariableIsSelected);
            this.DeleteVariableCommand = new RelayCommand(DeleteVariable, CustomVariableIsSelected);
        }

        // Named this method this way because we have a property of the same name. The RelayCommands need to specify
        // a method, not a property.
        private bool CustomVariableGroupIsSelectedMethod()
        {
            return this.SelectedCustomVariableGroup != null;
        }

        private void AddVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel();

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            this.SelectedCustomVariableGroup.CustomVariables.Add(viewModel.CustomVariable);

            SaveVariableGroup(() => this.SelectedCustomVariableGroup.CustomVariables.Remove(viewModel.CustomVariable));
        }

        private void EditVariable()
        {
            CustomVariableViewModel viewModel = new CustomVariableViewModel(this.SelectedCustomVariable);

            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);

            if (viewModel.UserCanceled) { return; }

            SaveVariableGroup(null);
        }

        private bool CustomVariableIsSelected()
        {
            return this.SelectedCustomVariable != null;
        }

        private void DeleteVariable()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariable.Key)) { return; }

            CustomVariable selectedCustomVariable = this.SelectedCustomVariable;

            this.SelectedCustomVariableGroup.CustomVariables.Remove(selectedCustomVariable);

            // Need to save the group. If we don't, then we'll get an exception when we access it again.
            // The group still thinks is has this variable until we save the group.
            SaveVariableGroup(() => this.SelectedCustomVariableGroup.CustomVariables.Add(selectedCustomVariable));
        }

        private void AddVariableGroup()
        {
            string newGroupName = "New group - " + DateTime.Now.ToString(CultureInfo.CurrentCulture);

            CustomVariableGroup newGroup = new CustomVariableGroup() { Name = newGroupName }; 

            this.CustomVariableGroups.Add(newGroup);

            this.SelectedCustomVariableGroup = this.CustomVariableGroups.Where(group => group.Name == newGroupName).FirstOrDefault();
        }        

        private void DeleteVariableGroup()
        {
            if (!UserConfirmsDelete(this.SelectedCustomVariableGroup.Name)) { return; }

            try
            {
                CustomVariableGroupLogic.Delete(this.SelectedCustomVariableGroup);
            }
            catch (Exception ex)
            {
                ViewModelUtility.MainWindowViewModel.UserMessage = ex.Message;

                ShowUserMessage(ex.Message, ViewModelResources.ItemCannotBeDeletedCaption);

                return;
            }

            this.CustomVariableGroups.Remove(this.SelectedCustomVariableGroup);
        }

        private void SaveVariableGroup(Action actionIfSaveFails)
        {
            try
            {
                CustomVariableGroupLogic.Save(this.SelectedCustomVariableGroup);
            }
            catch (ConcurrencyException)
            {
                string message = string.Format(CultureInfo.CurrentCulture, ViewModelResources.ItemCannotBeSavedConcurrency, this.SelectedCustomVariableGroup.Name);

                ViewModelUtility.MainWindowViewModel.UserMessage = message;

                ShowUserMessage(message, ViewModelResources.ItemNotSavedCaption);

                actionIfSaveFails.Invoke();
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemSaved, this.SelectedCustomVariableGroup.Name);
        }

        private void ImportVariableGroup()
        {
            string[] filePathAndNames = GetFilePathAndNamesFromUser();

            if (filePathAndNames == null || filePathAndNames.Count() < 1) { return; }

            CustomVariableGroup customVariableGroup;

            foreach (string filePathAndName in filePathAndNames)
            {
                using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Open))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomVariableGroup));
                    try
                    {
                        customVariableGroup = xmlSerializer.Deserialize(fileStream) as CustomVariableGroup;
                    }
                    catch (Exception ex)
                    {
                        ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                            ViewModelResources.CannotImport);
                        LogUtility.LogException(ex);
                        return;
                    }
                }

                // If the variable group name exists, append " - copy" to it.
                if (this.CustomVariableGroups.Any(group => group.Name == customVariableGroup.Name))
                {
                    customVariableGroup.Name += " - copy";
                }

                customVariableGroup.Id = null;  // This is new.
                CustomVariableGroupLogic.Save(customVariableGroup);
                this.CustomVariableGroups.Add(customVariableGroup);
            }
        }

        private void ExportVariableGroup()
        {
            string filePathAndName = SaveFilePathAndNameFromUser(this.SelectedCustomVariableGroup.Name + ".CustomVariableGroup");

            if (filePathAndName == null) { return; }

            using (FileStream fileStream = new FileStream(filePathAndName, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomVariableGroup));
                xmlSerializer.Serialize(fileStream, this.SelectedCustomVariableGroup);
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.ItemExported, this.SelectedCustomVariableGroup.Name);
        }

        private void RefreshVariableGroup()
        {
            this.LoadCustomVariableGroups();

            ViewModelUtility.MainWindowViewModel.UserMessage = "Items refreshed.";
        }   

        private void LoadCustomVariableGroups()
        {            
            try
            {
                this.CustomVariableGroups = new ObservableCollection<CustomVariableGroup>(CustomVariableGroupLogic.GetAll().ToList());
            }
            catch (Exception ex)
            {
                LogUtility.LogException(ex);
                ViewModelUtility.MainWindowViewModel.UserMessage = "Could not load form. Please see log for details.";
            }
        }
    }
}
