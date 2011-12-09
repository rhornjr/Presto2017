using System.Collections.ObjectModel;

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
    }
}
