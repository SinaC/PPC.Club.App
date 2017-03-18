using System.Runtime.Serialization;


namespace PPC.Data.Contracts
{
    [DataContract]
    public class CashRegisterCountEntry
    {
        [DataMember]
        public decimal Value { get; set; }
        [DataMember]
        public int Count { get; set; }
    }
}
