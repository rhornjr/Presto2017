
namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskXmlModify : TaskBase
    {
        /// <summary>
        /// Gets or sets the name of the XML path and file.
        /// </summary>
        /// <value>
        /// The name of the XML path and file.
        /// </value>
        public string XmlPathAndFileName { get; set; }

        /// <summary>
        /// Gets or sets the node to change.
        /// </summary>
        /// <value>
        /// The node to change.
        /// </value>
        public string NodeToChange { get; set; }

        /// <summary>
        /// Gets or sets the attribute key.
        /// </summary>
        /// <value>
        /// The attribute key.
        /// </value>
        public string AttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the attribute key value.
        /// </summary>
        /// <value>
        /// The attribute key value.
        /// </value>
        public string AttributeKeyValue { get; set; }

        /// <summary>
        /// Gets or sets the attribute to change.
        /// </summary>
        /// <value>
        /// The attribute to change.
        /// </value>
        public string AttributeToChange { get; set; }

        /// <summary>
        /// Gets or sets the attribute to change value.
        /// </summary>
        /// <value>
        /// The attribute to change value.
        /// </value>
        public string AttributeToChangeValue { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute()
        {
            // ToDo: Implement this.
        }
    }
}
