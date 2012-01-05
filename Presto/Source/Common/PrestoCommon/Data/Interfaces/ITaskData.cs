using PrestoCommon.Entities;

namespace PrestoCommon.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITaskData
    {
        /// <summary>
        /// Saves the specified task base.
        /// </summary>
        /// <param name="taskBase">The task base.</param>
        void Save(TaskBase taskBase);
    }
}
