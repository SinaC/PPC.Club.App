using System;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
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

        public override string ToString()
        {
            return $"{Quantity,-3} * {Ean,-15} {Description,-30} {Price,-10:C}";
        }
    }
}
