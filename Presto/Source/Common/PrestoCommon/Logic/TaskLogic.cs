using PrestoCommon.Data;
using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class TaskLogic
    {
        /// <summary>
        /// Saves the specified task base.
        /// </summary>
        /// <param name="taskBase">The task base.</param>
        public static void Save(TaskBase taskBase)
        {
            DataAccessFactory.GetDataInterface<ITaskData>().Save(taskBase);
        }
    }
}
