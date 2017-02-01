using System;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class Article
    {
        [DataMember]
        public Guid Guid { get; set; } // Grrrr
        [DataMember]
        public string Ean { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Producer { get; set; }
        [DataMember]
        public decimal SupplierPrice { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public int Stock { get; set; }
        [DataMember]
        public VatRates VatRate { get; set; }
        [DataMember]
        public bool IsNewArticle { get; set; }
    }
}
