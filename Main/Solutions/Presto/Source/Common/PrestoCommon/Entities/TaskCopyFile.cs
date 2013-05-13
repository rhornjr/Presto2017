using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using PrestoCommon.Enums;
using PrestoCommon.Misc;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class TaskCopyFile : TaskBase
    {
        [DataMember]
        public string SourcePath { get; set; }

        [DataMember]
        public string SourceFileName { get; set; }

        [DataMember]
        public string DestinationPath { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Desc")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "PrestoCommon.Misc.Logger.LogInformation(System.String)")]
        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            string sourcePath      = this.SourcePath;
            string sourceFileName  = this.SourceFileName;
            string destinationPath = this.DestinationPath;
            string status          = "Succeeded";

            try
            {
                sourcePath      = CustomVariableGroup.ResolveCustomVariable(this.SourcePath, applicationServer, applicationWithOverrideVariableGroup);
                sourceFileName  = CustomVariableGroup.ResolveCustomVariable(this.SourceFileName, applicationServer, applicationWithOverrideVariableGroup);
                destinationPath = CustomVariableGroup.ResolveCustomVariable(this.DestinationPath, applicationServer, applicationWithOverrideVariableGroup);

                List<string> listOfFilesToCopy = new List<string>();

                listOfFilesToCopy.AddRange(Directory.GetFiles(sourcePath, sourceFileName));  // Supports wildcards

                string fileNameOnly = string.Empty;

                foreach (string fileToCopy in listOfFilesToCopy)
                {
                    fileNameOnly = fileToCopy.Substring(fileToCopy.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);  // Get just the file name
                    if (!destinationPath.EndsWith(@"\", StringComparison.OrdinalIgnoreCase)) { destinationPath += @"\"; }
                    File.Copy(fileToCopy, destinationPath + fileNameOnly, true);
                }

                this.TaskSucceeded = true;
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                status = ex.Message;
                Logger.LogException(ex);
            }
            finally
            {                
                string message = "Copy File\r\n" +
                                 "Task Desc  : " + this.Description + "\r\n" +
                                 "Source     : " + sourcePath + @"\" + sourceFileName + "\r\n" +
                                 "Destination: " + destinationPath + "\r\n" +
                                 "Result     : " + status;

                this.TaskDetails = message;
                Logger.LogInformation(message);
            }
        }

        /// <summary>
        /// Creates the copy from this.
        /// </summary>
        /// <returns></returns>
        public TaskCopyFile CreateCopyFromThis()
        {
            TaskCopyFile newTaskCopyFile = new TaskCopyFile();

            return Copy(this, newTaskCopyFile);
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static TaskCopyFile Copy(TaskCopyFile source, TaskCopyFile destination)
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
            destination.DestinationPath = source.DestinationPath;
            destination.SourceFileName  = source.SourceFileName;
            destination.SourcePath      = source.SourcePath;

            return destination;
        }

        /// <summary>
        /// Creates the new from legacy task.
        /// </summary>
        /// <param name="legacyTaskBase">The legacy task base.</param>
        /// <returns></returns>
        public static TaskCopyFile CreateNewFromLegacyTask(PrestoCommon.Entities.LegacyPresto.LegacyTaskBase legacyTaskBase)
        {
            if (legacyTaskBase == null) { throw new ArgumentNullException("legacyTaskBase"); }

            PrestoCommon.Entities.LegacyPresto.LegacyTaskCopyFile legacyTask = legacyTaskBase as PrestoCommon.Entities.LegacyPresto.LegacyTaskCopyFile;

            TaskCopyFile newTask = new TaskCopyFile();

            // Base class
            newTask.Description          = legacyTask.Description;
            newTask.FailureCausesAllStop = legacyTask.FailureCausesAllStop;
            newTask.Sequence             = 0;
            newTask.TaskSucceeded        = false;
            newTask.PrestoTaskType       = TaskType.CopyFile;

            // Subclass
            newTask.DestinationPath = legacyTask.DestinationPath;
            newTask.SourceFileName  = legacyTask.SourceFileName;
            newTask.SourcePath      = legacyTask.SourcePath;

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

            taskProperties.Add(this.DestinationPath);
            taskProperties.Add(this.SourceFileName);
            taskProperties.Add(this.SourcePath);

            return taskProperties;
        }
    }
}
