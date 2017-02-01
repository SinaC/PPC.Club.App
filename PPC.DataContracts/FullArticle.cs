using System;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class FullArticle
    {
        [DataMember]
        public Guid Guid { get; set; }
        [DataMember]
        public string Ean { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
