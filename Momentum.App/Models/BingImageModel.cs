using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Momentum.App.Models
{
    [DataContract]
    public class BingImageModel
    {
        [DataMember]
        public List<ImageModel> images { get; set; }

        [DataMember]
        public TooltipsModel tooltips { get; set; }
    }
}
