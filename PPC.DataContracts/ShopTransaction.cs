using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class ShopTransaction
    {
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public List<Item> Articles { get; set; }
        [DataMember]
        public decimal Cash { get; set; }
        [DataMember]
        public decimal BankCard { get; set; }
    }
}
