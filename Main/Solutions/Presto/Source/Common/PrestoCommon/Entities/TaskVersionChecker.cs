using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    public class TaskVersionChecker : TaskBase
    {
        private string _fileName;
        private string _sourceFolder;
        private string _destinationFolder;

        public string FileName
        {
            get { return this._fileName; }

            set
            {
                this._fileName = value;
                NotifyPropertyChanged(() => this.FileName);
            }
        }

        public string SourceFolder
        {
            get { return this._sourceFolder; }

            set
            {
                this._sourceFolder = value;
                NotifyPropertyChanged(() => this.SourceFolder);
            }
        }

        public string DestinationFolder
        {
            get { return this._destinationFolder; }

            set
            {
                this._destinationFolder = value;
                NotifyPropertyChanged(() => this.DestinationFolder);
            }
        }

        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            string sourceFileVersion = string.Empty;
            string destinationFileVersion = string.Empty;

            try
            {
                sourceFileVersion = FileVersionInfo.GetVersionInfo(this.SourceFolder + @"\" + this.FileName).FileVersion;
                destinationFileVersion = FileVersionInfo.GetVersionInfo(this.DestinationFolder + @"\" + this.FileName).FileVersion;

                this.TaskSucceeded = false;  // default
                if (sourceFileVersion == destinationFileVersion) { this.TaskSucceeded = true; }
            }
            catch (Exception ex)
            {
                this.TaskSucceeded = false;
                this.TaskDetails = ex.Message + Environment.NewLine;
                LogUtility.LogException(ex);
            }
            finally
            {
                string logMessage = string.Format(CultureInfo.CurrentCulture,
                    PrestoCommonResources.TaskVersionCheckerLogMessage,
                    this.Description,
                    this.FileName,
                    sourceFileVersion,
                    destinationFileVersion);
                this.TaskDetails += logMessage;
                LogUtility.LogInformation(logMessage);
            }
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
            TaskVersionChecker newChecker = new TaskVersionChecker();

            // Base class
            newChecker.Description          = this.Description;
            newChecker.FailureCausesAllStop = this.FailureCausesAllStop;
            newChecker.Sequence             = this.Sequence;
            newChecker.TaskSucceeded        = this.TaskSucceeded;
            newChecker.PrestoTaskType       = this.PrestoTaskType;

            // Subclass
            newChecker.FileName          = this.FileName;
            newChecker.SourceFolder      = this.SourceFolder;
            newChecker.DestinationFolder = this.DestinationFolder;

            return newChecker;
        }
    }
}
