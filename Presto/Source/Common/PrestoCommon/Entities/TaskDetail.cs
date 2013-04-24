using System;
using System.Runtime.Serialization;

namespace PrestoCommon.Entities
{
    [DataContract]
    public class TaskDetail
    {
        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [DataMember]
        public string Details { get; set; }

        public TaskDetail(DateTime startTime, DateTime endTime, string details)
        {
            this.StartTime = startTime;
            this.EndTime   = endTime;
            this.Details   = details;
        }
    }
}
