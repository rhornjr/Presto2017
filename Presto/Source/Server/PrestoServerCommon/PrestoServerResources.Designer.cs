﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrestoServer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PrestoServerResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PrestoServerResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PrestoServer.PrestoServerResources", typeof(PrestoServerResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} installed on {1}, from {2} to {3}. Result: {4}.
        /// </summary>
        internal static string ApplicationInstalled {
            get {
                return ResourceManager.GetString("ApplicationInstalled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Application {0} will be installed on app server {1}.
        /// </summary>
        internal static string AppWillBeInstalledOnAppServer {
            get {
                return ResourceManager.GetString("AppWillBeInstalledOnAppServer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In the global setting entity, FreezeAllInstallations is true, so the installation will not happen: {0} on {1}..
        /// </summary>
        internal static string FreezeAllInstallationsTrueSoNoInstallation {
            get {
                return ResourceManager.GetString("FreezeAllInstallationsTrueSoNoInstallation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The global setting entity is null, so the installation will not happen: {0} on {1}..
        /// </summary>
        internal static string GlobalSettingNullSoNoInstallation {
            get {
                return ResourceManager.GetString("GlobalSettingNullSoNoInstallation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} was not saved because another user has changed it. Please refresh and try again..
        /// </summary>
        internal static string ItemCannotBeSavedConcurrency {
            get {
                return ResourceManager.GetString("ItemCannotBeSavedConcurrency", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An attempt was made to install the Presto self-updating application, but that application ({0}) was not found in the database..
        /// </summary>
        internal static string PrestoSelfUpdaterAppNotFound {
            get {
                return ResourceManager.GetString("PrestoSelfUpdaterAppNotFound", resourceCulture);
            }
        }
    }
}
