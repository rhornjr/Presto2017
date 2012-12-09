
namespace PrestoServerCommon.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IStartStop
    {
        /// <summary>
        /// Gets or sets the comment from service host.
        /// </summary>
        /// <value>
        /// The comment from service host.
        /// </value>
        string CommentFromServiceHost { get; set; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}
