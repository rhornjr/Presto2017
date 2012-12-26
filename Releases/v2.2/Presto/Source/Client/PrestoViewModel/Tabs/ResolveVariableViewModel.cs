﻿using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Exceptions;
using PrestoCommon.Logic;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Tabs
{
    /// <summary>
    /// 
    /// </summary>
    public class ResolveVariableViewModel : ViewModelBase
    {
        private PrestoObservableCollection<CustomVariable> _resolvedCustomVariables = new PrestoObservableCollection<CustomVariable>();
        private ApplicationServer _applicationServer = new ApplicationServer();
        private ApplicationWithOverrideVariableGroup _applicationWithOverrideVariableGroup = new ApplicationWithOverrideVariableGroup();

        /// <summary>
        /// Gets or sets the resolved custom variables.
        /// </summary>
        /// <value>
        /// The resolved custom variables.
        /// </value>
        public PrestoObservableCollection<CustomVariable> ResolvedCustomVariables
        {
            get { return this._resolvedCustomVariables; }

            set
            {
                this._resolvedCustomVariables = value;
                NotifyPropertyChanged(() => this.ResolvedCustomVariables);
            }
        }

        /// <summary>
        /// Gets or sets the application with group.
        /// </summary>
        /// <value>
        /// The application with group.
        /// </value>
        public ApplicationWithOverrideVariableGroup ApplicationWithGroup
        {
            get { return this._applicationWithOverrideVariableGroup; }

            set
            {
                this._applicationWithOverrideVariableGroup = value;
                NotifyPropertyChanged(() => this.ApplicationWithGroup);
            }
        }

        /// <summary>
        /// Gets or sets the application server.
        /// </summary>
        /// <value>
        /// The application server.
        /// </value>
        public ApplicationServer ApplicationServer
        {
            get { return this._applicationServer; }

            set
            {
                this._applicationServer = value;
                NotifyPropertyChanged(() => this.ApplicationServer);
            }
        }

        /// <summary>
        /// Gets or sets the select application command.
        /// </summary>
        /// <value>
        /// The select application command.
        /// </value>
        public ICommand SelectApplicationCommand { get; set; }

        /// <summary>
        /// Gets or sets the select group command.
        /// </summary>
        /// <value>
        /// The select group command.
        /// </value>
        public ICommand SelectGroupCommand { get; set; }

        /// <summary>
        /// Gets or sets the remove group command.
        /// </summary>
        /// <value>
        /// The remove group command.
        /// </value>
        public ICommand RemoveGroupCommand { get; set; }

        /// <summary>
        /// Gets or sets the select server command.
        /// </summary>
        /// <value>
        /// The select server command.
        /// </value>
        public ICommand SelectServerCommand { get; set; }

        /// <summary>
        /// Gets or sets the resolve command.
        /// </summary>
        /// <value>
        /// The resolve command.
        /// </value>
        public ICommand ResolveCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveVariableViewModel"/> class.
        /// </summary>
        public ResolveVariableViewModel()
        {
            if (DesignMode.IsInDesignMode) { return; }

            Initialize();
        }

        private void Initialize()
        {
            this.SelectApplicationCommand = new RelayCommand(SelectApplication);
            this.SelectGroupCommand       = new RelayCommand(SelectGroup);
            this.RemoveGroupCommand       = new RelayCommand(RemoveGroup);
            this.SelectServerCommand      = new RelayCommand(SelectServer);
            this.ResolveCommand           = new RelayCommand(Resolve, CanResolve);
        }

        private bool CanResolve()
        {
            // At a minimum, app and server need to be set.

            return this.ApplicationWithGroup != null && this.ApplicationWithGroup.Application != null
                && this.ApplicationServer != null;
        }

        private void SelectApplication()
        {
            ApplicationSelectorViewModel viewModel = new ApplicationSelectorViewModel();
            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);
            if (viewModel.UserCanceled) { return; }

            this.ApplicationWithGroup.Application = viewModel.SelectedApplication;
        }

        private void SelectGroup()
        {
            CustomVariableGroupSelectorViewModel groupViewModel = new CustomVariableGroupSelectorViewModel(false);
            MainWindowViewModel.ViewLoader.ShowDialog(groupViewModel);

            if (groupViewModel.UserCanceled) { return; }
            
            this.ApplicationWithGroup.CustomVariableGroup = groupViewModel.SelectedCustomVariableGroups[0];
        }

        private void RemoveGroup()
        {
            this.ApplicationWithGroup.CustomVariableGroup = null;
        }    

        private void SelectServer()
        {
            ApplicationServerSelectorViewModel viewModel = new ApplicationServerSelectorViewModel();
            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);
            if (viewModel.UserCanceled) { return; }

            this.ApplicationServer = viewModel.SelectedServer;
        }

        private void Resolve()
        {
            int numberOfProblemsFound = 0;
            bool variableFoundMoreThanOnce = false;
            this.ResolvedCustomVariables.Clear();
            string supplementalStatusMessage = string.Empty;

            // Need to get the latest app, group, and server each time we do this. The user could have made changes to them
            // since originally running this.
            RefreshAppGroupAndServer();

            foreach (TaskBase task in this.ApplicationWithGroup.Application.Tasks)
            {
                if (variableFoundMoreThanOnce) { break; }

                foreach (string taskProperty in task.GetTaskProperties())
                {
                    if (variableFoundMoreThanOnce) { break; }

                    string key   = string.Empty;
                    string value = string.Empty;

                    foreach (Match match in CustomVariableGroup.GetCustomVariableStringsWithinBiggerString(taskProperty))
                    {
                        // Don't add the same key more than once.
                        if (this.ResolvedCustomVariables.Any(x => x.Key == match.Value)) { continue; }

                        try
                        {
                            key = match.Value;
                            value = CustomVariableGroup.ResolveCustomVariable(match.Value, this.ApplicationServer, this.ApplicationWithGroup);
                        }
                        catch (CustomVariableMissingException)
                        {
                            numberOfProblemsFound++;
                            value = "** NOT FOUND **";
                        }
                        catch (CustomVariableExistsMoreThanOnceException ex)
                        {
                            variableFoundMoreThanOnce = true;
                            numberOfProblemsFound++;
                            value = "** MORE THAN ONE FOUND **";
                            supplementalStatusMessage = ex.Message;
                            this.ResolvedCustomVariables.Clear();
                            break;
                        }

                        this.ResolvedCustomVariables.Add(new CustomVariable() { Key = key, Value = value });
                    }                    
                }
            }

            ViewModelUtility.MainWindowViewModel.UserMessage = string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.VariablesResolved,
                numberOfProblemsFound.ToString(CultureInfo.CurrentCulture),
                supplementalStatusMessage);
        }

        private void RefreshAppGroupAndServer()
        {
            // Refresh the entities that were previously selected by the user.

            this.ApplicationWithGroup.Application = ApplicationLogic.GetById(this.ApplicationWithGroup.Application.Id);

            if (this.ApplicationWithGroup.CustomVariableGroup != null)
            {
                this.ApplicationWithGroup.CustomVariableGroup = CustomVariableGroupLogic.GetById(this.ApplicationWithGroup.CustomVariableGroup.Id);
            }

            this.ApplicationServer = ApplicationServerLogic.GetById(this.ApplicationServer.Id);
        }
    }
}