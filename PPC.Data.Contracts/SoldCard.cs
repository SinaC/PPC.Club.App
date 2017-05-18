using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class SoldCard
    {
        [DataMember]
        public string CardName { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public int Quantity { get; set; }

        public decimal Total => Quantity*Price;
    }
}
