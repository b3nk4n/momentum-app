using System;
using System.Runtime.Serialization;

namespace Momentum.App.Models
{
    [DataContract]
    public class TodaysFocusModel
    {
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}
