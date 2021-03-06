﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using PrestoCommon.Enums;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class TaskXmlModify : TaskBase
    {
        private string _xmlPathAndFileName;

        [DataMember]
        public string XmlPathAndFileName
        {
            get { return this._xmlPathAndFileName; }

            set
            {
                this._xmlPathAndFileName = value;
                this.NotifyPropertyChanged(() => this.XmlPathAndFileName);
            }
        }

        [DataMember]
        public string NodeNamespace { get; set; }

        [DataMember]
        public string NodeToChange { get; set; }

        [DataMember]
        public string AttributeKey { get; set; }

        [DataMember]
        public string AttributeKeyValue { get; set; }

        [DataMember]
        public string AttributeToChange { get; set; }

        [DataMember]
        public string AttributeToChangeValue { get; set; }

        [DataMember]
        public bool AddNode { get; set; }

        public TaskXmlModify()
            : base(string.Empty, TaskType.XmlModify, 1, 1, false)
        {}

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

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                string prefix = "";  // default
                string prefixSuffix = "";  // yes, a suffix for a prefix. Eat me.
                if (!string.IsNullOrWhiteSpace(taskResolved.NodeNamespace))
                {
                    prefix = "nssnuh";  // use something that we'll never expect to see in an XML doc.
                    prefixSuffix = ":";
                    namespaceManager.AddNamespace(prefix, taskResolved.NodeNamespace);
                }

                if (this.AddNode)
                {
                    AddNewNode(xmlDocument, taskResolved, rootElement);
                }
                else
                {
                    ModifyExistingNode(taskResolved, rootElement, prefix, prefixSuffix, namespaceManager);
                }

                // If internal subset doesn't have a value, then make it null. Making it null will cause
                // <!DOCTYPE HTML> to be the result (which is what we want), instead of <!DOCTYPE HTML[]>.
                // The brackets enable quirks mode. We don't want quirks mode.
                // http://stackoverflow.com/q/38832479/279516
                // http://stackoverflow.com/a/16451790/279516
                if (xmlDocument.DocumentType != null && string.IsNullOrWhiteSpace(xmlDocument.DocumentType.InternalSubset))
                {
                    var name                               = xmlDocument.DocumentType.Name;
                    var publicId                           = xmlDocument.DocumentType.PublicId;
                    var systemId                           = xmlDocument.DocumentType.SystemId;
                    var parent                             = xmlDocument.DocumentType.ParentNode;
                    var documentTypeWithNullInternalSubset = xmlDocument.CreateDocumentType(name, publicId, systemId, null);
                    parent.ReplaceChild(documentTypeWithNullInternalSubset, xmlDocument.DocumentType);
                }

                xmlDocument.Save(taskResolved.XmlPathAndFileName);
                xmlDocument = null;

                this.TaskSucceeded = true;

                this.TaskDetails = taskDetails;
                Logger.LogInformation(taskDetails);
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                this.TaskDetails = ex.Message + Environment.NewLine + taskDetails;
                Logger.LogException(ex);
            }
        }

        private static void AddNewNode(XmlDocument xmlDocument, TaskXmlModify taskResolved, XmlElement rootElement)
        {
            int indexOfLastSlash = taskResolved.NodeToChange.LastIndexOf("/", StringComparison.Ordinal);

            DemandSlash(taskResolved, indexOfLastSlash);

            string parentElementXPath = taskResolved.NodeToChange.Substring(0, indexOfLastSlash);
            string newElementName = taskResolved.NodeToChange.Substring(indexOfLastSlash + 1);

            var parentElement = rootElement.SelectSingleNode(parentElementXPath);
            var element = xmlDocument.CreateElement(newElementName);

            DemandParentElement(taskResolved, parentElement, parentElementXPath);

            element.SetAttribute(taskResolved.AttributeKey, taskResolved.AttributeKeyValue);
            element.SetAttribute(taskResolved.AttributeToChange, taskResolved.AttributeToChangeValue);
            
            parentElement.AppendChild(element);
        }

        private static void DemandParentElement(TaskXmlModify taskResolved, XmlNode parentElement, string parentElementXPath)
        {
            if (parentElement == null)
            {
                string noParentWarning = string.Format(CultureInfo.CurrentCulture,
                    "When trying to add a new XML node, the parent node was not found. parentElementXPath: [{0}]. " +
                    "NodeToChange: [{1}]",
                    parentElementXPath,
                    taskResolved.NodeToChange);

                throw new InvalidOperationException(noParentWarning);
            }
        }

        private static void DemandSlash(TaskXmlModify taskResolved, int indexOfLastSlash)
        {
            if (indexOfLastSlash < 1)
            {
                string noSlashWarning = string.Format(CultureInfo.CurrentCulture,
                    "When trying to add a new XML node, no slash was found in NodeToChange. A slash is " +
                    "necessary because the part before the last slash is the parent node. The part after " +
                    "the last slash in the new node. NodeToChange: [{0}]",
                    taskResolved.NodeToChange);

                throw new InvalidOperationException(noSlashWarning);
            }
        }

        private static void ModifyExistingNode(TaskXmlModify taskResolved, XmlElement rootElement, string prefix,
            string prefixSuffix, XmlNamespaceManager namespaceManager)
        {
            // Get the node that the user wants to modify
            XmlNodeList xmlNodes;
            if (string.IsNullOrWhiteSpace(taskResolved.AttributeKey))
            {
                // No attributes necessary to differentiate this node from any others. Get the matching nodes.
                xmlNodes = rootElement.SelectNodes(prefix + prefixSuffix + taskResolved.NodeToChange, namespaceManager);
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

            if (xmlNodes == null || xmlNodes.Count == 0)
            {
                // If this is happening, see the comments section below for a possible reason:
                // -- xmlnode not found because of namespace attribute --
                throw new Exception("XML node not found.\r\n" /* + taskDetails */);
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
        }

        private static string ConvertTaskDetailsToString(TaskXmlModify taskXmlModify)
        {
            // Store this error to use when throwing exceptions.                
            string taskDetails = string.Format(CultureInfo.CurrentCulture,
                                                "Task Description         : {0}\r\n" +
                                                "XML File                 : {1}\r\n" +
                                                "Node namespace           : {2}\r\n" +
                                                "Node to Change           : {3}\r\n" +
                                                "Attribute Key            : {4}\r\n" +
                                                "Attribute Key Value      : {5}\r\n" +
                                                "Attribute to Change      : {6}\r\n" +
                                                "Attribute to Change Value: {7}\r\n" +
                                                "Add node                 : {8}\r\n",
                                                taskXmlModify.Description,
                                                taskXmlModify.XmlPathAndFileName,
                                                taskXmlModify.NodeNamespace,
                                                taskXmlModify.NodeToChange,
                                                taskXmlModify.AttributeKey,
                                                taskXmlModify.AttributeKeyValue,
                                                taskXmlModify.AttributeToChange,
                                                taskXmlModify.AttributeToChangeValue,
                                                taskXmlModify.AddNode);
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
            taskXmlModifyResolved.NodeNamespace          = CustomVariableGroup.ResolveCustomVariable(this.NodeNamespace, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.NodeToChange           = CustomVariableGroup.ResolveCustomVariable(this.NodeToChange, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.XmlPathAndFileName     = CustomVariableGroup.ResolveCustomVariable(this.XmlPathAndFileName, applicationServer, applicationWithOverrideVariableGroup);
            taskXmlModifyResolved.AddNode                = this.AddNode;

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
            destination.NodeNamespace          = source.NodeNamespace;
            destination.NodeToChange           = source.NodeToChange;
            destination.XmlPathAndFileName     = source.XmlPathAndFileName;
            destination.AddNode                = source.AddNode;

            return destination;
        }

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
            newTask.AddNode                = false;  // there is no legacy option for this

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
            taskProperties.Add(this.AddNode.ToString());

            return taskProperties;
        }
    }
}
