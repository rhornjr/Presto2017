
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
            : base(string.Empty, TaskType.XmlModify, 1, 1, false)
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
        public override void Execute(ApplicationServer applicationServer)
        {
            TaskXmlModify taskResolved = GetTaskXmlModifyWithCustomVariablesResolved(applicationServer);

            string taskDetails = ConvertTaskDetailsToString(taskResolved);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(taskResolved.XmlPathAndFileName);
                XmlElement rootElement = xmlDocument.DocumentElement;

                XmlNodeList xmlNodes;

                // Get the node that the user wants to modify
                if (string.IsNullOrEmpty(taskResolved.AttributeKey.Trim()))
                {
                    // No attributes necessary to differentiate this node from any others. Get the matching nodes.
                    xmlNodes = rootElement.SelectNodes(taskResolved.NodeToChange);
                }
                else
                {
                    // Get the nodes with the specified attributes
                    xmlNodes = rootElement.SelectNodes(string.Format(CultureInfo.InvariantCulture,
                                                                       "descendant::{0}[@{1}='{2}']",
                                                                       taskResolved.NodeToChange,
                                                                       taskResolved.AttributeKey,
                                                                       taskResolved.AttributeKeyValue));
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
                    if (string.IsNullOrEmpty(taskResolved.AttributeToChange))
                    {
                        // We're not changing an attribute of the node; we're changing the value (InnerText) of the node itself.
                        xmlNode.InnerText = taskResolved.AttributeToChangeValue;
                    }
                    else
                    {
                        if (xmlNode.Attributes[taskResolved.AttributeToChange] == null)
                        {
                            throw new Exception("Can't update Attribute to Change because it does not exist in the file.");
                        }

                        // The node has an attribute, so change the attribute's value.
                        xmlNode.Attributes[taskResolved.AttributeToChange].Value = taskResolved.AttributeToChangeValue;
                    }
                }

                xmlDocument.Save(taskResolved.XmlPathAndFileName);
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

        private TaskXmlModify GetTaskXmlModifyWithCustomVariablesResolved(ApplicationServer applicationServer)
        {
            TaskXmlModify taskXmlModifyResolved = new TaskXmlModify();

            taskXmlModifyResolved.AttributeKey           = applicationServer.ResolveCustomVariable(this.AttributeKey);
            taskXmlModifyResolved.AttributeKeyValue      = applicationServer.ResolveCustomVariable(this.AttributeKeyValue);
            taskXmlModifyResolved.AttributeToChange      = applicationServer.ResolveCustomVariable(this.AttributeToChange);
            taskXmlModifyResolved.AttributeToChangeValue = applicationServer.ResolveCustomVariable(this.AttributeToChangeValue);
            taskXmlModifyResolved.NodeToChange           = applicationServer.ResolveCustomVariable(this.NodeToChange);
            taskXmlModifyResolved.XmlPathAndFileName     = applicationServer.ResolveCustomVariable(this.XmlPathAndFileName);

            return taskXmlModifyResolved;
        }

        /// <summary>
        /// Creates the copy from this.
        /// </summary>
        /// <returns></returns>
        public TaskXmlModify CreateCopyFromThis()
        {
            TaskXmlModify newTaskXmlModify = new TaskXmlModify();

            return Copy(this, newTaskXmlModify);
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static TaskXmlModify Copy(TaskXmlModify source, TaskXmlModify destination)
        {
            if (source == null) { return null; }

            if (destination == null) { throw new ArgumentNullException("destination"); }

            // Base class
            destination.Description          = source.Description;
            destination.FailureCausesAllStop = source.FailureCausesAllStop;
            destination.Sequence             = source.Sequence;
            destination.TaskSucceeded        = source.TaskSucceeded;
            destination.PrestoTaskType       = source.PrestoTaskType;

            // Subclass
            destination.AttributeKey           = source.AttributeKey;
            destination.AttributeKeyValue      = source.AttributeKeyValue;
            destination.AttributeToChange      = source.AttributeToChange;
            destination.AttributeToChangeValue = source.AttributeToChangeValue;
            destination.NodeToChange           = source.NodeToChange;
            destination.XmlPathAndFileName     = source.XmlPathAndFileName;

            return destination;
        }
    }
}
