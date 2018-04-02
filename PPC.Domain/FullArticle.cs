using System;
using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class FullArticle
    {
        [DataMember]
        public Guid Guid { get; set; }
        [DataMember]
        public string Ean { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string SubCategory { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public int Quantity { get; set; }

        public override string ToString()
        {
            return $"{Quantity,-3} * {Ean,-15} {Description,-30} {Category,-20} {SubCategory,-20} {Price,-10:C}";
        }
    }
}
