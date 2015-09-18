using System.Runtime.Serialization;

namespace Momentum.App.Models
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
