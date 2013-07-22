using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Wcf;
using PrestoViewModel.Mvvm;
using PrestoViewModel.Windows;

namespace PrestoViewModel.Misc
{
    /// <summary>
    /// View model helper methods
    /// </summary>
    internal static class ViewModelUtility
    {
        private static List<AdGroupWithRoles> _adGroupRolesList = null; 

        public static List<AdGroupWithRoles> AdGroupRolesList
        {
            get
            {
                if (_adGroupRolesList == null)
                {
                    using (var prestoWcf = new PrestoWcf<ISecurityService>())
                    {
                        _adGroupRolesList = prestoWcf.Service.GetAllAdGroupWithRoles().ToList();
                    }
                }

                return _adGroupRolesList;
            }
        }

        private static WindowsPrincipal _prestoUser;

        internal static WindowsPrincipal PrestoUser
        {
            get
            {
                if (_prestoUser == null)
                {
                    _prestoUser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                }

                return _prestoUser;
            }
        }

        public static bool UserCanAccessEnvironment(AdGroupWithRoles groupWithRoles, InstallationEnvironment environment)
        {
            if (!PrestoUser.IsInRole(groupWithRoles.AdGroupName)) { return false; }

            bool environmentExistsInRoles = groupWithRoles.PrestoRoles.Exists(
                r => r.ToString().ToUpperInvariant() == ("Modify" + environment).ToUpperInvariant());

            bool adminExistsInRoles = groupWithRoles.PrestoRoles.Exists(r => r.ToString().ToUpperInvariant() == "ADMIN");

            if (environmentExistsInRoles || adminExistsInRoles) { return true; }

            return false;

            // Original
            //return PrestoUser.IsInRole(groupWithRoles.AdGroupName) &&
            //       (groupWithRoles.PrestoRoles.Exists(
            //           r => r.ToString().ToUpperInvariant() == ("Modify" + environment).ToUpperInvariant()) ||
            //        groupWithRoles.PrestoRoles.Exists(r => r.ToString().ToUpperInvariant() == "ADMIN"));
        }

        /// <summary>
        /// Gets or sets the main window view model.
        /// </summary>
        /// <value>
        /// The main window view model.
        /// </value>
        internal static MainWindowViewModel MainWindowViewModel { get; set; }

        internal static TaskViewModel GetViewModel(TaskType taskType)
        {
            Type taskViewModelType = GetViewModelType(taskType);

            return Activator.CreateInstance(taskViewModelType) as TaskViewModel;
        }

        /// <summary>
        /// Gets the type of the view model.
        /// </summary>
        /// <param name="taskType">Type of the task.</param>
        /// <param name="taskBase">The task base to be passed to the view model in the constructor.</param>
        /// <returns></returns>
        internal static TaskViewModel GetViewModel(TaskType taskType, TaskBase taskBase)
        {
            Type taskViewModelType = GetViewModelType(taskType);

            return Activator.CreateInstance(taskViewModelType, taskBase) as TaskViewModel;
        }

        private static Type GetViewModelType(TaskType taskType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            List<Type> types = new List<Type>(assembly.GetTypes());

            Attribute[] attributes;
            foreach (Type theType in types)
            {
                attributes = Attribute.GetCustomAttributes(theType);

                foreach (TaskTypeAttribute attribute in attributes.Where(attr => attr is TaskTypeAttribute))
                {
                    if (attribute.TaskType == taskType)
                    {
                        return theType;
                    }
                }
            }

            return null;
        }
    }
}
