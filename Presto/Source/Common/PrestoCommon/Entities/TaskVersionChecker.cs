using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class TaskVersionChecker : TaskBase
    {
        private string _fileName;
        private string _sourceFolder;
        private string _destinationFolder;

        [DataMember]
        public string FileName
        {
            get { return this._fileName; }

            set
            {
                this._fileName = value;
                NotifyPropertyChanged(() => this.FileName);
            }
        }

        [DataMember]
        public string SourceFolder
        {
            get { return this._sourceFolder; }

            set
            {
                this._sourceFolder = value;
                NotifyPropertyChanged(() => this.SourceFolder);
            }
        }

        [DataMember]
        public string DestinationFolder
        {
            get { return this._destinationFolder; }

            set
            {
                this._destinationFolder = value;
                NotifyPropertyChanged(() => this.DestinationFolder);
            }
        }

        public TaskVersionChecker()
        {
            this.Description = "Compare versions";
            this.PrestoTaskType = Enums.TaskType.VersionChecker;
        }

        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            string sourceFileVersion = string.Empty;
            string destinationFileVersion = string.Empty;

            string sourceFolder      = string.Empty;
            string destinationFolder = string.Empty;
            string fileName          = string.Empty;

            try
            {
                sourceFolder      = CustomVariableGroup.ResolveCustomVariable(this.SourceFolder, applicationServer, applicationWithOverrideVariableGroup);
                destinationFolder = CustomVariableGroup.ResolveCustomVariable(this.DestinationFolder, applicationServer, applicationWithOverrideVariableGroup);
                fileName          = CustomVariableGroup.ResolveCustomVariable(this.FileName, applicationServer, applicationWithOverrideVariableGroup);

                // Will throw if file doesn't exist
                string sourcePathAndFile = SetSourcePathAndFile(sourceFolder, fileName);

                sourceFileVersion = FileVersionInfo.GetVersionInfo(sourcePathAndFile).FileVersion;

                string destinationPathAndFile = destinationFolder + @"\" + fileName;

                // It's okay if the destination file doesn't exist. This just means (hopefully) that this is
                // the first time we're deploying this file.
                if (!File.Exists(destinationPathAndFile))
                {
                    this.TaskSucceeded = true;
                }
                else
                {
                    destinationFileVersion = FileVersionInfo.GetVersionInfo(destinationPathAndFile).FileVersion;

                    this.TaskSucceeded = false;  // default
                    // We want the file versions to be different. Otherwise we're just deploying the same thing again,
                    // which is what this is designed to detect.
                    if (sourceFileVersion != destinationFileVersion) { this.TaskSucceeded = true; }
                }                
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                this.TaskDetails = ex.Message + Environment.NewLine;
                Logger.LogException(ex);
            }
            finally
            {
                string logMessage = string.Format(CultureInfo.CurrentCulture,
                    PrestoCommonResources.TaskVersionCheckerLogMessage,
                    this.Description,
                    fileName,
                    sourceFileVersion,
                    sourceFolder,
                    destinationFileVersion,
                    destinationFolder);
                this.TaskDetails += logMessage;
                Logger.LogInformation(logMessage);
            }
        }

        private static string SetSourcePathAndFile(string sourceFolder, string fileName)
        {
            string sourcePathAndFile = sourceFolder + @"\" + fileName;

            if (!File.Exists(sourcePathAndFile))
            {
                string message = string.Format(CultureInfo.CurrentCulture,
                    "When trying to compare versions on file [{0}], the source file was not found: [{1}]",
                    fileName,
                    sourcePathAndFile);
                throw new ArgumentException(message);
            }
            return sourcePathAndFile;
        }

        /// <summary>
        /// Gets the task properties. Each concrete task will add a string to the list that is the value of each property in the task.
        /// For example, for a copy file task, this would return three strings: SourcePath, SourceFileName, and DestinationPath.
        /// This is done so that custom variables can be resolved all at once.
        /// </summary>
        public override List<string> GetTaskProperties()
        {
            List<string> taskProperties = new List<string>();

            taskProperties.Add(this.FileName);
            taskProperties.Add(this.SourceFolder);
            taskProperties.Add(this.DestinationFolder);

            return taskProperties;
        }

        public override string ToString()
        {
            if (this.FileName == null) { return string.Empty; }
            
            return this.FileName;
        }

        public TaskVersionChecker CreateCopyFromThis()
        {
            TaskVersionChecker versionChecker = new TaskVersionChecker();

            return Copy(this, versionChecker);
        }

        public static TaskVersionChecker Copy(TaskVersionChecker source, TaskVersionChecker destination)
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
            destination.FileName          = source.FileName;
            destination.SourceFolder      = source.SourceFolder;
            destination.DestinationFolder = source.DestinationFolder;

            return destination;
        }
    }
}
