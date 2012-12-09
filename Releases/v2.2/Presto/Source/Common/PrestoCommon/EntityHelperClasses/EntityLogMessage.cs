
namespace PrestoCommon.EntityHelperClasses
{
    /// <summary>
    /// Provides a way to associate a log message with a specific entity.
    /// </summary>
    public class EntityLogMessage
    {
        /// <summary>
        /// Gets or sets EntityId
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets LogMessage
        /// </summary>
        public string LogMessage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="logMessage"></param>
        public EntityLogMessage(string entityId, string logMessage)
        {
            this.EntityId   = entityId;
            this.LogMessage = logMessage;
        }
    }
}
