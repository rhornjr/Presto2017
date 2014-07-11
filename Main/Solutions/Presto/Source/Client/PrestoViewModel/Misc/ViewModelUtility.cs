using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using PrestoCommon.Entities;
using PrestoCommon.Enums;
using PrestoCommon.Interfaces;
using PrestoCommon.Misc;
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

        private static ActiveDirectoryInfo _adInfo;

        internal static ActiveDirectoryInfo AdInfo
        {
            get
            {
                if (_adInfo == null)
                {
                    using (var prestoWcf = new PrestoWcf<ISecurityService>())
                    {
                        _adInfo = prestoWcf.Service.GetActiveDirectoryInfo();
                    }

                    // If it's still null after making the WCF call, create it with security set to false.
                    // We do this so we don't keep trying to get this information repeatedly during view/view-model binding.
                    if (_adInfo == null)
                    {
                        _adInfo = new ActiveDirectoryInfo() {SecurityEnabled = false};
                    }
                }
                return _adInfo;
            }
        }

        private static readonly List<InstallationEnvironment> _allEnvironments = InitializeAllEnvironments();

        public static List<string> AllowedRoles { get; set; }

        public static List<InstallationEnvironment> AllowedEnvironments { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ViewModelUtility()
        {
            InitializeAllowedRoles();
            InitializeAllowedEnvironments();
        }

        public static bool UserIsPrestoAdmin { get; private set; }

        private static List<InstallationEnvironment> InitializeAllEnvironments()
        {
            List<InstallationEnvironment> allEnvironments;

            using (var prestoWcf = new PrestoWcf<IInstallationEnvironmentService>())
            {
                allEnvironments = prestoWcf.Service.GetAllInstallationEnvironments().OrderBy(x => x.LogicalOrder).ToList();
            }

            return allEnvironments;
        }

        private static void InitializeAllowedRoles()
        {
            AllowedRoles = new List<string>();

            // It's possible that security hasn't been set up yet, so no AdInfo will exist. Set all roles to allowed.
            if (AdInfo == null || AdInfo.SecurityEnabled == false)
            {
                foreach (var groupWithRoles in AdGroupRolesList)
                {
                    groupWithRoles.PrestoRoles.ForEach(x => AllowedRoles.Add(x.ToString()));
                    UserIsPrestoAdmin = true;
                }
                return;
            }

            string domainConnection = string.Format(CultureInfo.InvariantCulture, "{0}.{1}:{2}", AdInfo.Domain, AdInfo.DomainSuffix, AdInfo.DomainPort);
            string container = string.Format(CultureInfo.InvariantCulture, "dc={0},dc={1}", AdInfo.Domain, AdInfo.DomainSuffix);
            string adQueryUser = AdInfo.ActiveDirectoryAccountUser;
            string adQueryPassword = AesCrypto.Decrypt(AdInfo.ActiveDirectoryAccountPassword);

            using (var principalContext = new PrincipalContext(ContextType.Domain, domainConnection, container, adQueryUser, adQueryPassword))
            using (var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, PrestoUser.Identity.Name))
            {
                foreach (var groupWithRoles in AdGroupRolesList)
                {
                    if (!userPrincipal.IsMemberOf(principalContext, IdentityType.SamAccountName, groupWithRoles.AdGroupName)) { continue; }

                    // If we reach here, the user is in the group passed into this method.

                    groupWithRoles.PrestoRoles.ForEach(x => AllowedRoles.Add(x.ToString()));

                    if (groupWithRoles.PrestoRoles.Exists(r => r == PrestoRole.Admin)) { UserIsPrestoAdmin = true; }
                }
            }
        }

        private static void InitializeAllowedEnvironments()
        {
            // Cache the environment and access (true/false) so we don't constantly make an active directory call during view/view model binding.

            AllowedEnvironments = new List<InstallationEnvironment>();

            foreach (var environment in _allEnvironments)
            {
                bool environmentExistsInRoles = AllowedRoles.Exists(
                    r => r.ToString().ToUpperInvariant() == ("Modify" + environment).ToUpperInvariant());

                if (environmentExistsInRoles || UserIsPrestoAdmin)
                {
                    if (!AllowedEnvironments.Exists(x => x.Name == environment.Name))
                    {
                        AllowedEnvironments.Add(environment);
                    }
                }
            }
        }

        public static bool UserCanAccessEnvironment(InstallationEnvironment environment)
        {
            if (AdInfo == null || AdInfo.SecurityEnabled == false) { return true; } // Security not enabled, so let it fly.

            return AllowedEnvironments.Exists(x => x.Name == environment.Name);
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
