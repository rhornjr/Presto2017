
namespace PrestoCommon.Entities
{
    /// <summary>
    /// An application can be installed multiple times on any given app server. Using the canonical exmaple, each
    /// instance needs to be the same as the others except for installation path, and one or more custom variables.
    /// </summary>
    public class ApplicationWithOverrideVariableGroup
    {
        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        /// <value>
        /// The application.
        /// </value>
        public Application Application { get; set; }

        /// <summary>
        /// Gets or sets the custom variable group.
        /// </summary>
        /// <value>
        /// The custom variable group.
        /// </value>
        public CustomVariableGroup CustomVariableGroup { get; set; }
    }
}
