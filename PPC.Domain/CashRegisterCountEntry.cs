using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class CashRegisterCountEntry
    {
        [DataMember]
        public decimal Value { get; set; }
        [DataMember]
        public int Count { get; set; }
    }
}
