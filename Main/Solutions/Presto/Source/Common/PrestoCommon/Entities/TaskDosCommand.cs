using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using PrestoCommon.Enums;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Task for a DOS command (anything you can do at a command prompt)
    /// </summary>
    [DataContract]
    public class TaskDosCommand : TaskBase
    {
        private string _dosExecutable;
        private string _parameters;
        private int _afterTaskPauseInSeconds;

        private readonly int MaxAfterTaskPauseInSeconds = 120;

        [DataMember]
        public string DosExecutable
        {
            get { return this._dosExecutable; }

            set
            {
                this._dosExecutable = value;
                NotifyPropertyChanged(() => this.DosExecutable);
            }
        }

        [DataMember]
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
        /// <remarks>
        /// Some DOS commands return immediately, not allowing enough time to complete before moving
        /// to the next task. With this pause, the user can pause processing for a certain amount of
        /// time, giving the task a chance to fully complete.
        /// </remarks>
        [DataMember]
        public int AfterTaskPauseInSeconds
        {
            get { return this._afterTaskPauseInSeconds; }

            set
            {
                this._afterTaskPauseInSeconds = value;
                NotifyPropertyChanged(() => this.AfterTaskPauseInSeconds);
            }
        }

        [DataMember]
        public string RunAsUser { get; set; }

        [DataMember]
        public string RunAsPassword { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskDosCommand"/> class.
        /// </summary>
        public TaskDosCommand()
        {
            this.PrestoTaskType = TaskType.DosCommand;
        }

        public TaskDosCommand(string description, byte failureCausesAllStop, int sequence, bool taskSucceeded,
            string dosExecutable, string parameters)
            : base(description, TaskType.DosCommand, failureCausesAllStop, sequence, taskSucceeded)
        {
            this.DosExecutable = dosExecutable;
            this.Parameters    = parameters;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup appWithGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            using (Process process = new Process())
            {
                string processOutput = string.Empty;

                try
                {
                    process.StartInfo.FileName               = CustomVariableGroup.ResolveCustomVariable(this.DosExecutable, applicationServer, appWithGroup);
                    process.StartInfo.Arguments              = CustomVariableGroup.ResolveCustomVariable(this.Parameters, applicationServer, appWithGroup);
                    process.StartInfo.UseShellExecute        = false;
                    process.StartInfo.RedirectStandardError  = true;
                    process.StartInfo.RedirectStandardInput  = false;  // See Note 1 at the bottom of this file.
                    process.StartInfo.RedirectStandardOutput = true;

                    SetProcessCredentials(applicationServer, appWithGroup, process);

                    process.Start();

                    processOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();

                    PossiblyPause();

                    // Used to check process.ExitCode here. See Note 2 at the bottom of this file for notes.

                    this.TaskSucceeded = true;
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
                        PrestoCommonResources.TaskDosCommandLogMessage,
                        this.Description, process.StartInfo.FileName,
                        process.StartInfo.Arguments, processOutput);
                    this.TaskDetails += logMessage;
                    Logger.LogInformation(logMessage);
                }
            }
        }

        private void SetProcessCredentials(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup appWithGroup, Process process)
        {
            // If this task contains a user name, use those credentials.
            if (string.IsNullOrWhiteSpace(this.RunAsUser)) { return; }

            string domainAndUser = CustomVariableGroup.ResolveCustomVariable(this.RunAsUser, applicationServer, appWithGroup);

            int indexOfBackslash = domainAndUser.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);

            if (indexOfBackslash < 0)
            {
                throw new InvalidOperationException("User name missing backslash. Unable to parse domain name.");
            }

            string domain = domainAndUser.Substring(0, indexOfBackslash);
            string user = domainAndUser.Substring(indexOfBackslash + 1);

            process.StartInfo.Domain = domain;
            process.StartInfo.UserName = user;
            process.StartInfo.Password = new SecureString();

            string password = CustomVariableGroup.ResolveCustomVariable(this.RunAsPassword, applicationServer, appWithGroup);

            foreach (char c in password)
            {
                process.StartInfo.Password.AppendChar(c);
            }
        }

        private void PossiblyPause()
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
            destination.RunAsUser               = source.RunAsUser;
            destination.RunAsPassword           = source.RunAsPassword;

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
            newTask.RunAsUser     = null;
            newTask.RunAsPassword = null;

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
            taskProperties.Add(this.RunAsUser);
            taskProperties.Add(this.RunAsPassword);

            return taskProperties;
        }
    }
}

/*

Note 1:
    The original note is below. RedirectStandardInput was set to true until now (25-Apr-2013). When
    RedirectStandardInput was true, process.Start() would hang when running on a background thread.
    See http://stackoverflow.com/q/16202678/279516. The original reason RedirectStandardInput was
    set to true was because of a quirk with xcopy (see below). I just tested xcopy and it now works
    with RedirectStandardInput set to false, so I'm going to leave it that way. Hopefully the
    original xcopy quirk/problem simply doesn't exist anymore.

    Hack: xcopy won't work unless you also redirect standard input. See 
    http://www.tek-tips.com/viewthread.cfm?qid=1421150&page=12:
    "Someone mentioned a quirk of xcopy.exe on one of the MSDN forums. When we redirect
    output we have to redirect input too. If we don’t, it immediately and silently quits
    right after startup."

 Note 2:
    Now I see why I had this commented before. When we run a DOS command, it can return a non-zero exit
    code, even though everything is ok. For example, if we need to delete files in a directory, but that
    directory doesn't exist, then who cares. All is good. So we either need to make sure all DOS commands
    have an exit code of 0 (maybe by checking the the directory exists first), or just ignore the exit code.
    if (process.ExitCode == 0)
    {
        this.TaskSucceeded = true;
        return;                        
    }

*/