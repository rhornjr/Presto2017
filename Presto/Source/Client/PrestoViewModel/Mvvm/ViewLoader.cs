using System;
using System.Collections.Generic;
using System.Windows;

namespace PrestoViewModel.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewLoader
    {
        private Dictionary<Type, Type> ViewModelToViewMapping = new Dictionary<Type, Type>();

        private Dictionary<ViewModelBase, Window> ViewModelInstanceToViewInstanceMapping = new Dictionary<ViewModelBase, Window>();

        /// <summary>
        /// Registers the specified view model type.
        /// </summary>
        /// <param name="viewModelType">Type of the view model.</param>
        /// <param name="viewType">Type of the view.</param>
        public void Register(Type viewModelType, Type viewType)
        {
            this.ViewModelToViewMapping.Add(viewModelType, viewType);
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void ShowDialog(ViewModelBase viewModel)
        {
            if (viewModel == null) { throw new ArgumentNullException("viewModel"); }

            Type viewType = this.ViewModelToViewMapping[viewModel.GetType()];

            Window viewWindow = Activator.CreateInstance(viewType) as Window;

            viewWindow.DataContext = viewModel;

            // Store the instance of the view, so we can close it when a view model calls CloseView().
            this.ViewModelInstanceToViewInstanceMapping.Add(viewModel, viewWindow);

            viewWindow.ShowDialog();
        }

        /// <summary>
        /// Closes the view.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void CloseView(ViewModelBase viewModel)
        {
            this.ViewModelInstanceToViewInstanceMapping[viewModel].Close();
        }
    }
}
