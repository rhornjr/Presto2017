using PrestoCommon.Data.Interfaces;
using PrestoCommon.Entities;

namespace PrestoCommon.Data.RavenDb
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskData : DataAccessLayerBase, ITaskData
    {
        /// <summary>
        /// Saves the specified task base.
        /// </summary>
        /// <param name="taskBase">The task base.</param>
        public void Save(TaskBase taskBase)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Save(taskBase);
        }
    }
}
