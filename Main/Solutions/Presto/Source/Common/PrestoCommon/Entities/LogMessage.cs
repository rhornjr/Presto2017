using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class LogMessage : EntityBase
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime MessageCreatedTime { get; set; }

        [DataMember]
        public string UserName { get; set; }

        public LogMessage(string message, DateTime messageCreatedTime, string userName)
        {
            this.Message            = message;
            this.MessageCreatedTime = messageCreatedTime;
            this.UserName           = userName;
        }
    }
}
