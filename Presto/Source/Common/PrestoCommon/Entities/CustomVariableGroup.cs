using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Container for <see cref="CustomVariable"/>s
    /// </summary>
    public class CustomVariableGroup
    {
        private ObservableCollection<CustomVariable> _customVariables;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

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
        /// Resolves the custom variable.
        /// </summary>
        /// <param name="unresolvedCustomVariable">The custom variable string.</param>
        /// <returns></returns>
        public string ResolveCustomVariable(string unresolvedCustomVariable)
        {
            // If the string doesn't contain a custom variable, nothing to do... just return the string.
            if (!StringHasCustomVariable(unresolvedCustomVariable)) { return unresolvedCustomVariable; }

            // Custom variables look like this: $(variableKey). So let's add the prefix and suffix to each key.
            string prefix = "$(";
            string suffix = ")";

            StringBuilder stringNew = new StringBuilder(unresolvedCustomVariable);

            do
            {
                foreach (CustomVariable customVariable in this.CustomVariables)
                {
                    // customVariable.Value could actually contain more custom variables. That's why we need to keep looping.
                    stringNew.Replace(prefix + customVariable.Key + suffix, customVariable.Value);
                }
            }
            while (StringHasCustomVariable(stringNew.ToString()) == true);

            return stringNew.ToString();
        }

        private static bool StringHasCustomVariable(string sourceString)
        {
            MatchCollection matchCollection = GetMatchCollection(sourceString);

            return matchCollection != null && matchCollection.Count > 0;
        }

        private static MatchCollection GetMatchCollection(string sourceString)
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
