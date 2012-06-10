using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using PrestoCommon.Enums;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Task for a DOS command (anything you can do at a command prompt)
    /// </summary>
    [Table("TaskDosCommands")]  // EF
    public class TaskDosCommand : TaskBase
    {
        private string _dosExecutable;
        private string _parameters;
        private int _afterTaskPauseInSeconds;

        private readonly int MaxAfterTaskPauseInSeconds = 120;

        /// <summary>
        /// Gets or sets the dos executable.
        /// </summary>
        /// <value>
        /// The dos executable.
        /// </value>
        public string DosExecutable
        {
            get { return this._dosExecutable; }

            set
            {
                this._dosExecutable = value;
                NotifyPropertyChanged(() => this.DosExecutable);
            }
        }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string Parameters
        {
            get { return this._parameters; }

            set
            {
                this._parameters = value;
                NotifyPropertyChanged(() => this.Parameters);
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds to pause after a command is executed.
        /// </summary>
        /// <value>
        /// The after task pause in seconds.
        /// </value>
        /// <remarks>
        /// Some DOS commands return immediately, not allowing enough time to complete before moving
        /// to the next task. With this pause, the user can pause processing for a certain amount of
        /// time, giving the task a chance to fully complete.
        /// </remarks>
        public int AfterTaskPauseInSeconds
        {
            get { return this._afterTaskPauseInSeconds; }

            set
            {
                this._afterTaskPauseInSeconds = value;
                NotifyPropertyChanged(() => this.AfterTaskPauseInSeconds);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommand"/> class.
        /// </summary>
        public TaskDosCommand()
        {
            this.PrestoTaskType = TaskType.DosCommand;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommand"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="failureCausesAllStop">The failure causes all stop.</param>
        /// <param name="sequence">The sequence.</param>
        /// <param name="taskSucceeded">if set to <c>true</c> [task succeeded].</param>
        /// <param name="dosExecutable">The dos executable.</param>
        /// <param name="parameters">The parameters.</param>
        public TaskDosCommand(string description, byte failureCausesAllStop, int sequence, bool taskSucceeded,
            string dosExecutable, string parameters)
            : base(description, TaskType.DosCommand, failureCausesAllStop, sequence, taskSucceeded)
        {
            this.DosExecutable = dosExecutable;
            this.Parameters    = parameters;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            using (Process process = new Process())
            {
                string processOutput = string.Empty;

                try
                {
                    // Hack: xcopy won't work unless you also redirect standard input. See 
                    // http://www.tek-tips.com/viewthread.cfm?qid=1421150&page=12:
                    // "Someone mentioned a quirk of xcopy.exe on one of the MSDN forums. When we redirect
                    // output we have to redirect input too. If we don’t, it immediately and silently quits
                    // right after startup."

                    process.StartInfo.FileName               = CustomVariableGroup.ResolveCustomVariable(this.DosExecutable, applicationServer, applicationWithOverrideVariableGroup);
                    process.StartInfo.Arguments              = CustomVariableGroup.ResolveCustomVariable(this.Parameters, applicationServer, applicationWithOverrideVariableGroup);
                    process.StartInfo.UseShellExecute        = false;
                    process.StartInfo.RedirectStandardError  = true;
                    process.StartInfo.RedirectStandardInput  = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    processOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    Pause();

                    // Now I see why I had this commented before. When we run a DOS command, it can return a non-zero exit
                    // code, even though everything is ok. For example, if we need to delete files in a directory, but that
                    // directory doesn't exist, then who cares. All is good. So we either need to make sure all DOS commands
                    // have an exit code of 0 (maybe by checking the the directory exists first), or just ignore the exit code.
                    //if (process.ExitCode == 0)
                    //{
                    //    this.TaskSucceeded = true;
                    //    return;                        
                    //}

                    this.TaskSucceeded = true;

                    //LogUtility.LogWarning(string.Format(CultureInfo.CurrentCulture,
                    //    PrestoCommonResources.TaskDosCommandFailedWithExitCode,
                    //    process.ExitCode.ToString(CultureInfo.CurrentCulture)));
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
                        PrestoCommonResources.TaskDosCommandLogMessage,
                        this.Description, process.StartInfo.FileName,
                        process.StartInfo.Arguments, processOutput);
                    this.TaskDetails += logMessage;
                    LogUtility.LogInformation(logMessage);
                }
            }
        }

        private void Pause()
        {
            if (this.AfterTaskPauseInSeconds <= 0) { return; }  // No pause.

            Thread.Sleep(this.AfterTaskPauseInSeconds * 1000);
        }

        /// <summary>
        /// Creates a new instance of <see cref="TaskDosCommand"/>, setting all of the properties to equal
        /// the values of this instance.
        /// </summary>
        /// <returns></returns>
        public TaskDosCommand CreateCopyFromThis()
        {
            TaskDosCommand newTaskDosCommand = new TaskDosCommand();

            return Copy(this, newTaskDosCommand);
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns></returns>
        public static TaskDosCommand Copy(TaskDosCommand source, TaskDosCommand destination)
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
            destination.DosExecutable           = source.DosExecutable;
            destination.Parameters              = source.Parameters;
            destination.AfterTaskPauseInSeconds = source.AfterTaskPauseInSeconds;

            return destination;
        }

        /// <summary>
        /// Creates the new from legacy task.
        /// </summary>
        /// <param name="legacyTaskBase">The legacy task base.</param>
        /// <returns></returns>
        public static TaskDosCommand CreateNewFromLegacyTask(PrestoCommon.Entities.LegacyPresto.LegacyTaskBase legacyTaskBase)
        {
            if (legacyTaskBase == null) { throw new ArgumentNullException("legacyTaskBase"); }

            PrestoCommon.Entities.LegacyPresto.LegacyTaskDosCommand legacyTask = legacyTaskBase as PrestoCommon.Entities.LegacyPresto.LegacyTaskDosCommand;

            TaskDosCommand newTask = new TaskDosCommand();

            // Base class
            newTask.Description          = legacyTask.Description;
            newTask.FailureCausesAllStop = legacyTask.FailureCausesAllStop;
            newTask.Sequence             = 0;
            newTask.TaskSucceeded        = false;
            newTask.PrestoTaskType       = TaskType.DosCommand;

            // Subclass
            newTask.DosExecutable = legacyTask.DosExecutable;
            newTask.Parameters    = legacyTask.Parameters;

            return newTask;
        }

        /// <summary>
        /// Determines whether this instance is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            if (this.AfterTaskPauseInSeconds < 0 || this.AfterTaskPauseInSeconds > this.MaxAfterTaskPauseInSeconds) { return false; }

            return true;
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

            taskProperties.Add(this.DosExecutable);
            taskProperties.Add(this.Parameters);

            return taskProperties;
        }
    }
}
