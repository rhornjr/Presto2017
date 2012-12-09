using System;

namespace PrestoCommon.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class LogMessage : EntityBase
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message created time.
        /// </summary>
        /// <value>
        /// The message created time.
        /// </value>
        public DateTime MessageCreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageCreatedTime">The message created time.</param>
        /// <param name="userName">Name of the user.</param>
        public LogMessage(string message, DateTime messageCreatedTime, string userName)
        {
            this.Message            = message;
            this.MessageCreatedTime = messageCreatedTime;
            this.UserName           = userName;
        }
    }
}
