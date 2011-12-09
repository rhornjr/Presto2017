
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using PrestoCommon.Enums;
using PrestoCommon.Misc;
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
        /// Initializes a new instance of the <see cref="TaskXmlModify"/> class.
        /// </summary>
        public TaskXmlModify()
            : base("", TaskType.XmlModify, 1, 1, false)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskXmlModify"/> class.
        /// </summary>
        public TaskXmlModify(string description, byte failureCausesAllStop, int sequence, bool taskSucceeded,
            string xmlPathAndFileName, string nodeToChange, string attributeKey, string attributeKeyValue,
            string attributeToChange, string attributeToChangeValue)
            : base(description, TaskType.XmlModify, failureCausesAllStop, sequence, taskSucceeded)
        {
            this.XmlPathAndFileName     = xmlPathAndFileName;
            this.NodeToChange           = nodeToChange;
            this.AttributeKey           = attributeKey;
            this.AttributeKeyValue      = attributeKeyValue;
            this.AttributeToChange      = attributeToChange;
            this.AttributeToChangeValue = attributeToChangeValue;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public override void Execute()
        {
            // ToDo: Resolve custom variables
            //TaskXmlModify taskXmlModifyOriginal = task as TaskXmlModify;
            //TaskXmlModify taskXmlModify = GetTaskXmlModifyWithCustomVariablesResolved(taskXmlModifyOriginal);

            string taskDetails = ConvertTaskDetailsToString(this);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(this.XmlPathAndFileName);
                XmlElement rootElement = xmlDocument.DocumentElement;

                XmlNodeList xmlNodes;

                // Get the node that the user wants to modify
                if (string.IsNullOrEmpty(this.AttributeKey.Trim()))
                {
                    // No attributes necessary to differentiate this node from any others. Get the matching nodes.
                    xmlNodes = rootElement.SelectNodes(this.NodeToChange);
                }
                else
                {
                    // Get the nodes with the specified attributes
                    xmlNodes = rootElement.SelectNodes(string.Format(CultureInfo.InvariantCulture,
                                                                       "descendant::{0}[@{1}='{2}']",
                                                                       this.NodeToChange,
                                                                       this.AttributeKey,
                                                                       this.AttributeKeyValue));
                }

                if (xmlNodes == null)
                {
                    // If this is happening, see the comments section below for a possible reason:
                    // -- xmlnode not found because of namespace attribute --
                    throw new Exception("XML node not found.\r\n" /* + taskDetails */ );
                }

                // Make the change
                foreach (XmlNode xmlNode in xmlNodes)
                {
                    if (string.IsNullOrEmpty(this.AttributeToChange))
                    {
                        // We're not changing an attribute of the node; we're changing the value (InnerText) of the node itself.
                        xmlNode.InnerText = this.AttributeToChangeValue;
                    }
                    else
                    {
                        if (xmlNode.Attributes[this.AttributeToChange] == null)
                        {
                            throw new Exception("Can't update Attribute to Change because it does not exist in the file.");
                        }

                        // The node has an attribute, so change the attribute's value.
                        xmlNode.Attributes[this.AttributeToChange].Value = this.AttributeToChangeValue;
                    }
                }

                xmlDocument.Save(this.XmlPathAndFileName);
                xmlDocument = null;

                this.TaskSucceeded = true;

                LogUtility.LogInformation(taskDetails);
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                LogUtility.LogException(ex);
            }
        }

        private static string ConvertTaskDetailsToString(TaskXmlModify taskXmlModify)
        {
            // Store this error to use when throwing exceptions.                
            string taskDetails = string.Format(CultureInfo.CurrentCulture,
                                                "Task Description         : {0}\r\n" +
                                                "XML File                 : {1}\r\n" +
                                                "Node to Change           : {2}\r\n" +
                                                "Attribute Key            : {3}\r\n" +
                                                "Attribute Key Value      : {4}\r\n" +
                                                "Attribute to Change      : {5}\r\n" +
                                                "Attribute to Change Value: {6}\r\n",
                                                taskXmlModify.Description,
                                                taskXmlModify.XmlPathAndFileName,
                                                taskXmlModify.NodeToChange,
                                                taskXmlModify.AttributeKey,
                                                taskXmlModify.AttributeKeyValue,
                                                taskXmlModify.AttributeToChange,
                                                taskXmlModify.AttributeToChangeValue);
            return taskDetails;
        }
    }
}
