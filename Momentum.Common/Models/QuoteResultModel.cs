﻿using System.Runtime.Serialization;

namespace Momentum.Common.Models
{
    [DataContract]
    public class QuoteResultModel
    {
        [DataMember]
        public int code { get; set; }
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public QuoteDataModel data { get; set; }
    }
}
