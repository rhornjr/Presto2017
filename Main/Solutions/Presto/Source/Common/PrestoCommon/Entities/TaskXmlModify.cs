using System;
using System.Collections.Generic;
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
        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            string taskDetails = string.Empty;

            try
            {
                TaskXmlModify taskResolved = GetTaskXmlModifyWithCustomVariablesResolved(applicationServer, applicationWithOverrideVariableGroup);
                taskDetails = ConvertTaskDetailsToString(taskResolved);

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

                this.TaskDetails = taskDetails;
                LogUtility.LogInformation(taskDetails);
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                this.TaskDetails = ex.Message + Environment.NewLine + taskDetails;
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

        private TaskXmlModify GetTaskXmlModifyWithCustomVariablesResolved(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            TaskXmlModify taskXmlModifyResolved = new TaskXmlModify();

            taskXmlModifyResolved.Description          = this.Description;
            taskXmlModifyResolved.FailureCausesAllStop = this.FailureCausesAllStop;
            taskXmlModifyResolved.PrestoTaskType       = this.PrestoTaskType;
            taskXmlModifyResolved.Sequence             = this.Sequence;
            taskXmlModifyResolved.TaskSucceeded        = this.TaskSucceeded;

            taskXmlModifyResolved.AttributeKey           = CustomVariableGroup.ResolveCustomVariable(this.AttributeKey, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.AttributeKeyValue      = CustomVariableGroup.ResolveCustomVariable(this.AttributeKeyValue, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.AttributeToChange      = CustomVariableGroup.ResolveCustomVariable(this.AttributeToChange, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.AttributeToChangeValue = CustomVariableGroup.ResolveCustomVariable(this.AttributeToChangeValue, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.NodeToChange           = CustomVariableGroup.ResolveCustomVariable(this.NodeToChange, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.XmlPathAndFileName     = CustomVariableGroup.ResolveCustomVariable(this.XmlPathAndFileName, applicationServer, applicationWithOverrideVariableGroup);

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

        /// <summary>
        /// Creates the new from legacy task.
        /// </summary>
        /// <param name="legacyTaskBase">The legacy task base.</param>
        /// <returns></returns>
        public static TaskXmlModify CreateNewFromLegacyTask(PrestoCommon.Entities.LegacyPresto.LegacyTaskBase legacyTaskBase)
        {
            if (legacyTaskBase == null) { throw new ArgumentNullException("legacyTaskBase"); }

            PrestoCommon.Entities.LegacyPresto.LegacyTaskXmlModify legacyTask = legacyTaskBase as PrestoCommon.Entities.LegacyPresto.LegacyTaskXmlModify;

            TaskXmlModify newTask = new TaskXmlModify();

            // Base class
            newTask.Description          = legacyTask.Description;
            newTask.FailureCausesAllStop = legacyTask.FailureCausesAllStop;
            newTask.Sequence             = 0;
            newTask.TaskSucceeded        = false;
            newTask.PrestoTaskType       = TaskType.XmlModify;

            // Subclass
            newTask.AttributeKey           = legacyTask.AttributeKey;
            newTask.AttributeKeyValue      = legacyTask.AttributeKeyValue;
            newTask.AttributeToChange      = legacyTask.AttributeToChange;
            newTask.AttributeToChangeValue = legacyTask.AttributeToChangeValue;
            newTask.NodeToChange           = legacyTask.NodeToChange;
            newTask.XmlPathAndFileName     = legacyTask.XmlPathAndFileName;

            return newTask;
        }

        /// <summary>
        /// Gets the task properties. Each concrete task will add a string to the list that is the value of each property in the task.
        /// For example, for a copy file task, this would return three strings: SourcePath, SourceFileName, and DestinationPath.
        /// This is done so that custom variables can be resolved all at once.
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTaskProperties()
        {
            List<string> taskProperties = new List<string>();

            taskProperties.Add(this.AttributeKey);
            taskProperties.Add(this.AttributeKeyValue);
            taskProperties.Add(this.AttributeToChange);
            taskProperties.Add(this.AttributeToChangeValue);
            taskProperties.Add(this.NodeToChange);
            taskProperties.Add(this.XmlPathAndFileName);

            return taskProperties;
        }
    }
}
