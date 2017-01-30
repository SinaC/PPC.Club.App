using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class SoldArticle
    {
        [DataMember]
        public string Ean { get; set; }
        [DataMember]
        public int Quantity { get; set; }
        [DataMember]
        public bool IsCash { get; set; }
    }
}
