using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDosCommandLogic : LogicBase
    {
        /// <summary>
        /// Saves the specified task dos command.
        /// </summary>
        /// <param name="taskBase">The task base.</param>
        public static void Save(TaskBase taskBase)
        {
            TaskDosCommand taskDosCommand = taskBase as TaskDosCommand; // ToDo: Is this cast necessary?

            Database.Store(taskDosCommand);
            Database.Commit();
        }
    }
}
