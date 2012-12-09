using System;

namespace PrestoViewModel.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ViewModelAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of the view model.
        /// </summary>
        /// <value>
        /// The type of the view model.
        /// </value>
        public Type ViewModelType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelAttribute"/> class.
        /// </summary>
        /// <param name="viewModelType">Type of the view model.</param>
        public ViewModelAttribute(Type viewModelType)
        {
            this.ViewModelType = viewModelType;
        }
    }
}
