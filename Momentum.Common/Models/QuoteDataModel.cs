using System.Runtime.Serialization;

namespace Momentum.Common.Models
{
    [DataContract]
    public class QuoteDataModel
    {
        [DataMember]
        public string quote { get; set; }
        [DataMember]
        public string author { get; set; }
    }
}
