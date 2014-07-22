using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using PrestoCommon.Enums;
using Xanico.Core;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// Task to deploy a completely separate application. For example, if we have an app bundle (an app that
    /// has tasks to deploy many other apps), instead of including all of the tasks to install each app, we
    /// simply create a TaskApp that points to another app.
    /// </summary>
    [DataContract]
    public class TaskApp : TaskBase
    {
        private ApplicationWithOverrideVariableGroup _appWithGroup;

        [DataMember]
        public ApplicationWithOverrideVariableGroup AppWithGroup
        {
            get { return _appWithGroup; }
            
            set
            {
                _appWithGroup = value;
                if (_appWithGroup ==  null || _appWithGroup.Application == null) { return; }

                this.Description = _appWithGroup.Application.NameAndVersion;

                if (_appWithGroup.CustomVariableGroup != null) { this.Description += " - " + _appWithGroup.CustomVariableGroup.Name;}
            }
        }

        public TaskApp(ApplicationWithOverrideVariableGroup appWithGroup)
        {
            this.AppWithGroup = appWithGroup;
            this.PrestoTaskType = TaskType.App;
        }

        public override void Execute(ApplicationServer applicationServer, ApplicationWithOverrideVariableGroup applicationWithOverrideVariableGroup)
        {
            if (applicationServer == null) { throw new ArgumentNullException("applicationServer"); }

            try
            {
                this.AppWithGroup.Install(applicationServer, DateTime.Now, false);
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
                    PrestoCommonResources.TaskAppLogMessage,
                    this.Description);
                this.TaskDetails += logMessage;
                Logger.LogInformation(logMessage);
            }
        }

        /// <summary>
        /// Gets the task properties. Each concrete task will add a string to the list that is the value of each property in the task.
        /// For example, for a copy file task, this would return three strings: SourcePath, SourceFileName, and DestinationPath.
        /// This is done so that custom variables can be resolved all at once.
        /// </summary>
        public override List<string> GetTaskProperties()
        {
            return new List<string>();
        }
    }
}
