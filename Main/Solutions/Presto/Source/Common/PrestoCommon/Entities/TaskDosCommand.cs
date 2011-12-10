using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using PrestoCommon.Enums;
using PrestoCommon.Misc;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Task for a DOS command (anything you can do at a command prompt)
    /// </summary>
    public class TaskDosCommand : TaskBase
    {
        private string _dosExecutable;
        private string _parameters;

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
        public override void Execute(ApplicationServer applicationServer)
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

                    process.StartInfo.FileName               = applicationServer.ResolveCustomVariable(this.DosExecutable);
                    process.StartInfo.Arguments              = applicationServer.ResolveCustomVariable(this.Parameters);
                    process.StartInfo.UseShellExecute        = false;
                    process.StartInfo.RedirectStandardError  = true;
                    process.StartInfo.RedirectStandardInput  = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();

                    processOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        this.TaskSucceeded = true;
                        return;                        
                    }

                    this.TaskSucceeded = false;
                    LogUtility.LogWarning(string.Format(CultureInfo.CurrentCulture,
                        PrestoCommonResources.TaskDosCommandFailedWithExitCode,
                        process.ExitCode.ToString(CultureInfo.CurrentCulture)));
                }
                catch (Exception ex)
                {
                    this.TaskSucceeded = false;
                    LogUtility.LogException(ex);
                }
                finally
                {
                    string logMessage = string.Format(CultureInfo.CurrentCulture,
                        PrestoCommonResources.TaskDosCommandLogMessage,
                        this.Description, process.StartInfo.FileName,
                        process.StartInfo.Arguments, processOutput);

                    LogUtility.LogInformation(logMessage);
                }
            }
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

            destination.Description          = source.Description;
            destination.DosExecutable        = source.DosExecutable;
            destination.FailureCausesAllStop = source.FailureCausesAllStop;
            destination.Parameters           = source.Parameters;
            destination.Sequence             = source.Sequence;
            destination.TaskSucceeded        = source.TaskSucceeded;
            destination.PrestoTaskType             = source.PrestoTaskType;

            return destination;
        }
    }
}
