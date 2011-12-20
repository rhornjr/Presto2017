using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PrestoCommon.Logic;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Container for <see cref="CustomVariable"/>s
    /// </summary>
    public class CustomVariableGroup : EntityBase
    {
        private Application _application;
        private ObservableCollection<CustomVariable> _customVariables;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the application that's associated with this custom variable group. If an application
        /// is associated with a custom variable group, then only that application will be able to use it.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        [XmlIgnore]  // This is here for exporting custom variable groups. Application.Tasks is read only, so it couldn't serialize.
        public Application Application
        {
            get { return this._application; }

            set
            {
                this._application = value;
                NotifyPropertyChanged(() => this.Application);
            }
        }

        /// <summary>
        /// Gets or sets the custom variables.
        /// </summary>
        /// <value>
        /// The custom variables.
        /// </value>
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
        /// <param name="applicationWithOverrideVariableGroup">The application with override variable group.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "string")]
        public static string ResolveCustomVariable(string rawString, ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }
            if (applicationWithOverrideVariableGroup == null) { throw new ArgumentNullException("applicationWithOverrideVariableGroup"); }

            if (!StringHasCustomVariable(rawString)) { return rawString; }

            List<CustomVariable> allCustomVariables = new List<CustomVariable>();

            // First, get all custom variables associated with the app server.
            foreach (CustomVariableGroup customVariableGroup in applicationServer.CustomVariableGroups)
            {
                allCustomVariables.AddRange(customVariableGroup.CustomVariables);
            }

            // Get the custom variable group associated with this *application*.
            CustomVariableGroup appGroup = CustomVariableGroupLogic.Get(applicationWithOverrideVariableGroup.Application.Name);

            if (appGroup != null && appGroup.CustomVariables != null) { allCustomVariables.AddRange(appGroup.CustomVariables); }

            // Add the override custom variables. If they already exist in our list, replace them.
            AddRangeOverride(allCustomVariables, applicationWithOverrideVariableGroup.CustomVariableGroup.CustomVariables.ToList());

            if (!CustomVariableExistsInListOfAllCustomVariables(rawString, allCustomVariables))
            {
                string message = string.Format(CultureInfo.CurrentCulture,
                    "{0} contains a custom variable that does not exist in the list of custom variables.", rawString);
                LogUtility.LogWarning(message);
                throw new ArgumentException(message);
            }

            return ResolveCustomVariable(rawString, allCustomVariables);
        }

        private static void AddRangeOverride(List<CustomVariable> allCustomVariables, List<CustomVariable> newCustomVariables)
        {
            // Add the new custom variables to the list, overwriting any duplicates.

            // First, remove duplicates.
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

            // If this custom variable group can resolve any of these custom variables, return true.
            foreach (Match match in matchCollection)
            {
                string matchValueWithoutPrefixAndSuffix = CustomVariableWithoutPrefixAndSuffix(match.Value);
                if (allCustomVariables.Where(customVariable => customVariable.Key == matchValueWithoutPrefixAndSuffix).FirstOrDefault() != null)
                { return true; }
            }

            return false;
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
                foreach (CustomVariable customVariable in allCustomVariables)
                {
                    // customVariable.Value could actually contain more custom variables. That's why we need to keep looping.
                    stringNew.Replace(prefix + customVariable.Key + suffix, customVariable.Value);
                }
            }
            while (StringHasCustomVariable(stringNew.ToString()));

            return stringNew.ToString();
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

        private static MatchCollection GetCustomVariableStringsWithinBiggerString(string sourceString)
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
