using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PrestoCommon.Enums;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskCopyFile : TaskBase
    {
        /// <summary>
        /// Gets or sets the source path.
        /// </summary>
        /// <value>
        /// The source path.
        /// </value>
        public string SourcePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the source file.
        /// </summary>
        /// <value>
        /// The name of the source file.
        /// </value>
        public string SourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the destination path.
        /// </summary>
        /// <value>
        /// The destination path.
        /// </value>
        public string DestinationPath { get; set; }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="applicationServer"></param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Desc")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "PrestoCommon.Misc.LogUtility.LogInformation(System.String)")]
        public override void Execute(ApplicationServer applicationServer)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            string sourcePath      = this.SourcePath;
            string sourceFileName  = this.SourceFileName;
            string destinationPath = this.DestinationPath;
            string status          = "Succeeded";

            try
            {
                sourcePath      = applicationServer.ResolveCustomVariable(this.SourcePath);
                sourceFileName  = applicationServer.ResolveCustomVariable(this.SourceFileName);
                destinationPath = applicationServer.ResolveCustomVariable(this.DestinationPath);

                List<string> listOfFilesToCopy = new List<string>();

                listOfFilesToCopy.AddRange(Directory.GetFiles(sourcePath, sourceFileName));  // Supports wildcards

                string fileNameOnly = string.Empty;

                foreach (string fileToCopy in listOfFilesToCopy)
                {
                    fileNameOnly = fileToCopy.Substring(fileToCopy.LastIndexOf(@"\", StringComparison.OrdinalIgnoreCase) + 1);  // Get just the file name
                    File.Copy(fileToCopy, destinationPath + @"\" + fileNameOnly, true);
                }

                this.TaskSucceeded = true;
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                status = ex.Message;
                LogUtility.LogException(ex);
            }
            finally
            {                
                string message = "Copy File\r\n" +
                                 "Task Desc  : " + this.Description + "\r\n" +
                                 "Source     : " + sourcePath + @"\" + sourceFileName + "\r\n" +
                                 "Destination: " + destinationPath + "\r\n" +
                                 "Result     : " + status;

                LogUtility.LogInformation(message);
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
        public static TaskCopyFile CreateNewFromLegacyTask(PrestoCommon.Entities.LegacyPresto.TaskBase legacyTaskBase)
        {
            if (legacyTaskBase == null) { throw new ArgumentNullException("legacyTaskBase"); }

            PrestoCommon.Entities.LegacyPresto.TaskCopyFile legacyTask = legacyTaskBase as PrestoCommon.Entities.LegacyPresto.TaskCopyFile;

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
    }
}
