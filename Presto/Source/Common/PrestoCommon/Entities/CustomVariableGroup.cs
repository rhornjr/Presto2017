using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PrestoCommon.Exceptions;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Container for <see cref="CustomVariable"/>s
    /// </summary>
    public class CustomVariableGroup : EntityBase
    {
        private string _name;
        private ObservableCollection<CustomVariable> _customVariables;        

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        [JsonIgnore]
        public ICollection<Application> Applications { get; set; }

        /// <summary>
        /// Gets or sets the custom variables.
        /// </summary>
        /// <value>
        /// The custom variables.
        /// </value>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Deserialization")]
        public ObservableCollection<CustomVariable> CustomVariables
        {
            get
            {
                if (this._customVariables == null)
                {
                    this._customVariables = new ObservableCollection<CustomVariable>();
                }
                return this._customVariables;
            }

            set
            {
                this._customVariables = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Resolves the custom variable.
        /// </summary>
        /// <param name="rawString">The raw string.</param>
        /// <param name="applicationServer">The application server.</param>
        /// <param name="appWithGroup">The application with override variable group.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static string ResolveCustomVariable(string rawString, ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }

            if (!StringHasCustomVariable(rawString)) { return rawString; }

            List<CustomVariable> allCustomVariables = new List<CustomVariable>();

            // Add system variables
            AddSystemVariables(allCustomVariables, applicationServer, appWithGroup);

            // Add all custom variables associated with the app server.
            foreach (CustomVariableGroup customVariableGroup in applicationServer.CustomVariableGroups)
            {
                ThrowIfDuplicateCustomVariableKeyExists(allCustomVariables, customVariableGroup);
                allCustomVariables.AddRange(customVariableGroup.CustomVariables);
            }

            // Add all custom variables associated with the application.
            foreach (CustomVariableGroup customVariableGroup in appWithGroup.Application.CustomVariableGroups)
            {
                ThrowIfDuplicateCustomVariableKeyExists(allCustomVariables, customVariableGroup);
                allCustomVariables.AddRange(customVariableGroup.CustomVariables);
            }

            // Add the override custom variables. If they already exist in our list, replace them.
            // Since these are overrides, duplicates are okay here.
            AddRangeOverride(allCustomVariables, appWithGroup);

            if (!CustomVariableExistsInListOfAllCustomVariables(rawString, allCustomVariables))
            { LogMissingVariableAndThrow(rawString); }

            return ResolveCustomVariable(rawString, allCustomVariables);
        }

        private static void ThrowIfDuplicateCustomVariableKeyExists(List<CustomVariable> allCustomVariables, CustomVariableGroup customVariableGroup)
        {
            string duplicateKey = DuplicateCustomVariableKey(allCustomVariables, customVariableGroup);
            
            if (!string.IsNullOrEmpty(duplicateKey))
            {
                LogDuplicateVariableAndThrow(duplicateKey);
            }
        }

        private static string DuplicateCustomVariableKey(IEnumerable<CustomVariable> masterVariableList, CustomVariableGroup newGroup)
        {
            foreach (CustomVariable customVariable in newGroup.CustomVariables)
            {
                if (masterVariableList.Any(x => x.Key == customVariable.Key)) { return customVariable.Key; }
            }

            return string.Empty;
        }

        private static void AddSystemVariables(List<CustomVariable> allCustomVariables, ApplicationServer applicationServer,
            ApplicationWithOverrideVariableGroup appWithOverrideGroup)
        {
            allCustomVariables.Add(new CustomVariable() { Key = "sys:applicationName",    Value = appWithOverrideGroup.Application.Name });
            allCustomVariables.Add(new CustomVariable() { Key = "sys:applicationVersion", Value = appWithOverrideGroup.Application.Version });
            allCustomVariables.Add(new CustomVariable() { Key = "sys:serverName",         Value = applicationServer.Name });
        }

        private static void AddRangeOverride(List<CustomVariable> allCustomVariables, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationWithOverrideVariableGroup.CustomVariableGroup == null || applicationWithOverrideVariableGroup.CustomVariableGroup.CustomVariables == null)
            {
                return;  // No custom variable group to add to main list.
            }

            // Add the new custom variables to the list, overwriting any duplicates.

            List<CustomVariable> newCustomVariables = applicationWithOverrideVariableGroup.CustomVariableGroup.CustomVariables.ToList();

            // First, remove variables that are the same as the new/override variables.
            foreach (CustomVariable newCustomVariable in newCustomVariables)
            {
                CustomVariable customVariable = allCustomVariables.Where(variable => variable.Key == newCustomVariable.Key).FirstOrDefault();
                if (customVariable != null) { allCustomVariables.Remove(customVariable); }
            }

            // Now add all the new variables.
            allCustomVariables.AddRange(newCustomVariables);
        }

        private static bool CustomVariableExistsInListOfAllCustomVariables(string stringNew, IEnumerable<CustomVariable> allCustomVariables)
        {
            // The raw string could be something like this: ServiceName $(site)
            // This call should return a collection of found custom variables, like $(site), within the raw string.
            MatchCollection matchCollection = GetCustomVariableStringsWithinBiggerString(stringNew);

            // If this custom variable group can't resolve any of these custom variables, return false.
            foreach (Match match in matchCollection)
            {
                string matchValueWithoutPrefixAndSuffix = CustomVariableWithoutPrefixAndSuffix(match.Value);
                if (allCustomVariables.Where(customVariable => customVariable.Key == matchValueWithoutPrefixAndSuffix).FirstOrDefault() == null)
                { return false; }
            }

            return true;
        }

        /// <summary>
        /// Resolves the custom variable.
        /// </summary>
        /// <param name="rawString">The custom variable string.</param>
        /// <param name="allCustomVariables">All custom variables.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        private static string ResolveCustomVariable(string rawString, IEnumerable<CustomVariable> allCustomVariables)
        {
            // If the string doesn't contain a custom variable, nothing to do... just return the string.
            if (!StringHasCustomVariable(rawString)) { return rawString; }

            // Custom variables look like this: $(variableKey). So let's add the prefix and suffix to each key.
            string prefix = "$(";
            string suffix = ")";

            StringBuilder stringNew = new StringBuilder(rawString);

            do
            {
                // Since we can actually introduce new custom variables, we need to check if a new
                // variable exists in the list. (A new custom variable can reference yet another custom variable.)
                if (!CustomVariableExistsInListOfAllCustomVariables(stringNew.ToString(), allCustomVariables))
                { LogMissingVariableAndThrow(stringNew.ToString()); }

                foreach (CustomVariable customVariable in allCustomVariables)
                {
                    // customVariable.Value could actually contain more custom variables. That's why we need to keep looping.
                    stringNew.Replace(prefix + customVariable.Key + suffix, customVariable.Value);
                }
            }
            while (StringHasCustomVariable(stringNew.ToString()));

            return stringNew.ToString();
        }

        private static void LogMissingVariableAndThrow(string rawString)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                    "{0} contains a custom variable that does not exist in the list of custom variables.", rawString);
            LogUtility.LogWarning(message);
            throw new CustomVariableMissingException(message);
        }

        private static void LogDuplicateVariableAndThrow(string customVariableKey)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                    "A custom variable key ({0}) exists more than once. Only one is allowed.", customVariableKey);
            LogUtility.LogWarning(message);
            throw new CustomVariableExistsMoreThanOnceException(message);
        }

        private static string CustomVariableWithoutPrefixAndSuffix(string customVariableString)
        {
            // Custom variables look like this: $(variableKey).

            string customVariableKeyWithoutPrefix = customVariableString.Substring(2);
            string customVariableKeyWithoutPrefixAndSuffix =
                customVariableKeyWithoutPrefix.Remove(customVariableKeyWithoutPrefix.Length - 1, 1);  // Remove trailing ")"

            return customVariableKeyWithoutPrefixAndSuffix;
        }

        private static bool StringHasCustomVariable(string sourceString)
        {
            MatchCollection matchCollection = GetCustomVariableStringsWithinBiggerString(sourceString);

            return matchCollection != null && matchCollection.Count > 0;
        }

        public static MatchCollection GetCustomVariableStringsWithinBiggerString(string sourceString)
        {
            // Use a regex to find all custom variables in sourceString. The pattern is $(variableName).

            // Explanation of regular expression below:
            // \$  : finds the dollar sign (has to be escaped with the slash)
            // \(  : finds the left parenthesis
            // .+? : . matches any character, + means one or more, ? means ungreedy (will consume characters until the FIRST occurrence of the right parenthesis)
            // \)  : finds the right parenthesis
            Regex regex = new Regex(@"\$\(.+?\)");

            return regex.Matches(sourceString);
        }
    }
}
