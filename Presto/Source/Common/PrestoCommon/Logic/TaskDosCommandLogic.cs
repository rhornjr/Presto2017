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
        /// <param name="taskDosCommand">The task dos command.</param>
        public static void Save(TaskDosCommand taskDosCommand)
        {
            Database.Store(taskDosCommand);
            Database.Commit();
        }
    }
}
