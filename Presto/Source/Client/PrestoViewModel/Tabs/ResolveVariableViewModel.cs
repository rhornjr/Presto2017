﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using PrestoCommon.Entities;
using PrestoCommon.EntityHelperClasses;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
using PrestoCommon.Wcf;
using PrestoViewModel.Misc;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Tabs
{
    public class ResolveVariableViewModel : ViewModelBase
    {
        private PrestoObservableCollection<CustomVariable> _resolvedCustomVariables = new PrestoObservableCollection<CustomVariable>();
        private ApplicationServer _applicationServer;
        private ApplicationWithOverrideVariableGroup _applicationWithOverrideVariableGroup = new ApplicationWithOverrideVariableGroup();

        public PrestoObservableCollection<CustomVariable> ResolvedCustomVariables
        {
            get { return this._resolvedCustomVariables; }

            set
            {
                this._resolvedCustomVariables = value;
                NotifyPropertyChanged(() => this.ResolvedCustomVariables);
            }
        }

        private PrestoObservableCollection<CustomVariableGroup> _selectedCustomVariableGroups;
        
        public ApplicationWithOverrideVariableGroup ApplicationWithGroup
        {
            get { return this._applicationWithOverrideVariableGroup; }

            set
            {
                this._applicationWithOverrideVariableGroup = value;
                NotifyPropertyChanged(() => this.ApplicationWithGroup);
            }
        }

        public ApplicationServer ApplicationServer
        {
            get { return this._applicationServer; }

            set
            {
                this._applicationServer = value;
                NotifyPropertyChanged(() => this.ApplicationServer);
            }
        }

        public ICommand SelectApplicationCommand { get; set; }

        public ICommand SelectGroupCommand { get; set; }

        public ICommand RemoveGroupCommand { get; set; }

        public ICommand SelectServerCommand { get; set; }

        public ICommand ResolveCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveVariableViewModel"/> class.
        /// </summary>
        public ResolveVariableViewModel()
        {
            Debug.WriteLine("ResolveVariableViewModel constructor start " + DateTime.Now);

            if (DesignMode.IsInDesignMode) { return; }

            Initialize();

            Debug.WriteLine("ResolveVariableViewModel constructor end " + DateTime.Now);
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
            this.ResolvedCustomVariables.Clear();
        }

        private void SelectGroup()
        {
            CustomVariableGroupSelectorViewModel groupViewModel = new CustomVariableGroupSelectorViewModel(_selectedCustomVariableGroups);
            MainWindowViewModel.ViewLoader.ShowDialog(groupViewModel);

            if (groupViewModel.UserCanceled) { return; }
            
            // Store the (possibly) multiple selected groups.
            _selectedCustomVariableGroups = groupViewModel.SelectedCustomVariableGroups;
            this.ApplicationWithGroup.CustomVariableGroups = groupViewModel.SelectedCustomVariableGroups;
            
            this.ResolvedCustomVariables.Clear();
        }

        private void RemoveGroup()
        {
            // The reason we create a new PrestoObservableCollection here is because the setter will also
            // notify that the CustomVariableGroupNames property also changed.
            this.ApplicationWithGroup.CustomVariableGroups = new PrestoObservableCollection<CustomVariableGroup>();
            this.ResolvedCustomVariables.Clear();

            //_selectedCustomVariableGroupIds.Clear();
            _selectedCustomVariableGroups.Clear();
        }    

        private void SelectServer()
        {
            ApplicationServerSelectorViewModel viewModel = new ApplicationServerSelectorViewModel();
            MainWindowViewModel.ViewLoader.ShowDialog(viewModel);
            if (viewModel.UserCanceled) { return; }

            this.ApplicationServer = viewModel.SelectedServer;
            this.ResolvedCustomVariables.Clear();
        }

        private void Resolve()
        {
            RefreshAppGroupAndServer();

            var resolvedVariablesContainer = VariableGroupResolver.Resolve(this.ApplicationWithGroup, this.ApplicationServer);

            this.ResolvedCustomVariables.Clear();
            this.ResolvedCustomVariables.AddRange(resolvedVariablesContainer.Variables);

            ViewModelUtility.MainWindowViewModel.AddUserMessage(string.Format(CultureInfo.CurrentCulture,
                ViewModelResources.VariablesResolved,
                resolvedVariablesContainer.NumberOfProblems.ToString(CultureInfo.CurrentCulture),
                resolvedVariablesContainer.SupplementalStatusMessage));
        }

        private void RefreshAppGroupAndServer()
        {
            // Refresh the entities that were previously selected by the user.

            using (var prestoWcf = new PrestoWcf<IApplicationService>())
            {
                this.ApplicationWithGroup.Application =
                    prestoWcf.Service.GetById(this.ApplicationWithGroup.Application.Id);
            }

            if (this.ApplicationWithGroup.CustomVariableGroups != null)
            {
                var cvgIds = this.ApplicationWithGroup.CustomVariableGroups.Select(x => x.Id).ToList();
                this.ApplicationWithGroup.CustomVariableGroups.ClearItemsAndNotifyChangeOnlyWhenDone();
                using (var prestoWcf = new PrestoWcf<ICustomVariableGroupService>())
                {
                    foreach (var cvgId in cvgIds)
                    {
                        this.ApplicationWithGroup.CustomVariableGroups.Add(prestoWcf.Service.GetById(cvgId));
                    }
                }
            }

            using (var prestoWcf = new PrestoWcf<IServerService>())
            {
                this.ApplicationServer = prestoWcf.Service.GetServerById(this.ApplicationServer.Id);
            }
        }
    }
}
