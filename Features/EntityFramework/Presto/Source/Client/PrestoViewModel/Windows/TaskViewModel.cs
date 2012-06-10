using PrestoCommon.Entities;

namespace PrestoViewModel.Windows
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TaskViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets a value indicating whether [user canceled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user canceled]; otherwise, <c>false</c>.
        /// </value>
        public bool UserCanceled { get; protected set; }

        /// <summary>
        /// Gets the task base.
        /// </summary>
        public TaskBase TaskBase { get; protected set; }
    }
}
