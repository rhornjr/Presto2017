using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using PrestoCommon.Exceptions;
using PrestoCommon.Misc;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class CustomVariableGroup : EntityBase
    {
        private string _name;
        private ObservableCollection<CustomVariable> _customVariables;

        [DataMember]
        public bool Disabled { get; set; }

        [DataMember]
        public bool Deleted { get; set; }  // Since there is no referential integrity in RavenDB, use this property.

        [DataMember]
        public string Name
        {
            get { return this._name; }

            set
            {
                this._name = value;
                NotifyPropertyChanged(() => this.Name);
            }
        }

        [DataMember]
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

        public override string ToString()
        {
            return this.Name;
        }

        private static string ObjectDump(CustomVariableGroup customVariableGroup)
        {
            var dump = new StringBuilder();

            dump.AppendLine(customVariableGroup.Name);

            dump.AppendLine();
            dump.AppendLine("Custom Variables:");
            dump.AppendLine();

            foreach (var customVariable in customVariableGroup.CustomVariables)
            {
                dump.AppendLine(customVariable.Key + ": " + customVariable.Value);
            }

            return dump.ToString();
        }

        public static string DifferencesBetweenTwoCustomVariableGroups(CustomVariableGroup oldGroup, CustomVariableGroup newGroup)
        {
            if (newGroup == null) { throw new ArgumentNullException("newGroup"); }

            if (oldGroup == null) { return "New group created" + Environment.NewLine + Environment.NewLine  + ObjectDump(newGroup); }

            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("DELETED");
            stringBuilder.AppendLine("Variables in the old group, but not in the new group:");
            foreach (var oldVariable in oldGroup.CustomVariables)
            {
                var newVariable = newGroup.CustomVariables.FirstOrDefault(x => x.Key == oldVariable.Key);
                if (newVariable == null)
                {
                    stringBuilder.AppendLine(oldVariable.Key + ": " + oldVariable.Value);
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("ADDED");
            stringBuilder.AppendLine("Variables in the new group, but not in the old group:");
            foreach (var newVariable in newGroup.CustomVariables)
            {
                var oldVariable = oldGroup.CustomVariables.FirstOrDefault(x => x.Key == newVariable.Key);
                if (oldVariable == null)
                {
                    stringBuilder.AppendLine(newVariable.Key + ": " + newVariable.Value);
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("CHANGED");
            stringBuilder.AppendLine("Variables that exist in the old and new groups, but are different:");
            foreach (var oldVariable in oldGroup.CustomVariables)
            {
                var newVariable = newGroup.CustomVariables.FirstOrDefault(x => x.Key == oldVariable.Key);
                if (newVariable == null) { continue; }
                if (oldVariable.Value == newVariable.Value) { continue; }

                stringBuilder.AppendLine("Key: " + oldVariable.Key);
                stringBuilder.AppendLine("Old value: " + oldVariable.Value);
                stringBuilder.AppendLine("New value: " + newVariable.Value);
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static string ResolveCustomVariable(string rawString, ApplicationServer applicationServer,
            ApplicationWithOverrideVariableGroup appWithGroup, bool leaveValueEncrypted = false)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }

            if (String.IsNullOrWhiteSpace(rawString)) { return rawString; }

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

            return ResolveCustomVariable(rawString, allCustomVariables, leaveValueEncrypted);
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
            allCustomVariables.Add(new CustomVariable() { Key = "sys:applicationName",       Value = appWithOverrideGroup.Application.Name });
            allCustomVariables.Add(new CustomVariable() { Key = "sys:applicationVersion",    Value = appWithOverrideGroup.Application.Version });
            allCustomVariables.Add(new CustomVariable() { Key = "sys:serverName",            Value = applicationServer.Name });
            allCustomVariables.Add(new CustomVariable() { Key = "sys:installationTimestamp", Value = ApplicationWithOverrideVariableGroup.InstallationStartTimestamp });
        }

        private static void AddRangeOverride(List<CustomVariable> allCustomVariables, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationWithOverrideVariableGroup.CustomVariableGroups == null
                || applicationWithOverrideVariableGroup.CustomVariableGroups.Count < 1)
            {
                return;  // No custom variable group to add to main list.
            }

            // Add the new custom variables to the list, overwriting any duplicates.

            // ... but before we add the overrides, need to make sure the overrides don't
            // have any duplicates within themselves.
            var hashset = new HashSet<string>();
            foreach (var cvg in applicationWithOverrideVariableGroup.CustomVariableGroups)
            {
                foreach (var cv in cvg.CustomVariables)
                {
                    if (!hashset.Add(cv.Key))
                    {
                        LogDuplicateVariableAndThrow(cv.Key);
                    }
                }
            }

            var newCustomVariables = new List<CustomVariable>();
            
            foreach (var cvg in applicationWithOverrideVariableGroup.CustomVariableGroups)
            {
                newCustomVariables.AddRange(cvg.CustomVariables.ToList());
            }

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
        private static string ResolveCustomVariable(string rawString, IEnumerable<CustomVariable> allCustomVariables, bool leaveValueEncrypted)
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
                    stringNew.Replace(prefix + customVariable.Key + suffix,
                        DecryptedCustomVariableValue(customVariable, leaveValueEncrypted));
                }
            }
            while (StringHasCustomVariable(stringNew.ToString()));

            return stringNew.ToString();
        }

        private static string DecryptedCustomVariableValue(CustomVariable customVariable, bool leaveValueEncrypted)
        {
            if (!customVariable.ValueIsEncrypted || leaveValueEncrypted) { return customVariable.Value; }

            return AesCrypto.Decrypt(customVariable.Value);
        }

        private static void LogMissingVariableAndThrow(string rawString)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                    "{0} contains a custom variable that does not exist in the list of custom variables.", rawString);
            Logger.LogWarning(message);
            throw new CustomVariableMissingException(message);
        }

        private static void LogDuplicateVariableAndThrow(string customVariableKey)
        {
            string message = string.Format(CultureInfo.CurrentCulture,
                    "A custom variable key ({0}) exists more than once. Only one is allowed.", customVariableKey);
            Logger.LogWarning(message);
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
            // A null won't work with regex.Matches. So use an empty string so an empty MatchCollection
            // will be returned.
            if (sourceString == null) { sourceString = string.Empty; }

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
