using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class ClientCart
    {
        [DataMember]
        public string ClientName { get; set; }
        [DataMember]
        public List<Item> Articles { get; set; }
        [DataMember]
        public bool IsPaid { get; set; }
        [DataMember]
        public decimal Cash { get; set; }
        [DataMember]
        public decimal BankCard { get; set; }
    }
}
