using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrestoCommon.Enums;
using PrestoViewModel.Mvvm;

namespace PrestoViewModel.Misc
{
    /// <summary>
    /// View model helper methods
    /// </summary>
    internal static class ViewModelUtility
    {
        /// <summary>
        /// Gets or sets the main window view model.
        /// </summary>
        /// <value>
        /// The main window view model.
        /// </value>
        internal static MainWindowViewModel MainWindowViewModel { get; set; }

        internal static Type GetViewModel(TaskType taskType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            List<Type> types = new List<Type>(assembly.GetTypes());

            Attribute[] attributes;
            foreach (Type theType in types)
            {
                attributes = Attribute.GetCustomAttributes(theType);

                foreach (TaskTypeAttribute attribute in attributes.Where(attr => attr is TaskTypeAttribute))
                {
                    if (attribute.TaskType == taskType) { return theType; }
                }
            }

            return default(Type);
        }
    }
}
