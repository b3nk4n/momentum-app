
using System.Runtime.Serialization;

namespace Momentum.Common.Models
{
    [DataContract]
    public class MessageModel
    {
        public string title { get; set; }
        public string link { get; set; }
        public string text { get; set; }
    }
}
