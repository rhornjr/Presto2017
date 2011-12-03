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
        /// <summary>
        /// Gets or sets the dos executable.
        /// </summary>
        /// <value>
        /// The dos executable.
        /// </value>
        public string DosExecutable { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public string Parameters { get; set; }

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
        public override void Execute()
        {
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

                    // ToDo: ReplaceVariablesWithValues for the first two lines below.
                    process.StartInfo.FileName               = this.DosExecutable;
                    process.StartInfo.Arguments              = this.Parameters;
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
    }
}
