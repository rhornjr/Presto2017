using System.Collections.ObjectModel;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Container for <see cref="CustomVariable"/>s
    /// </summary>
    public class CustomVariableGroup
    {
        private Collection<CustomVariable> _customVariables;

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
        public Collection<CustomVariable> CustomVariables
        {
            get
            {
                if (this._customVariables == null)
                {
                    this._customVariables = new Collection<CustomVariable>();
                }
                return this._customVariables;
            }
        }
    }
}
