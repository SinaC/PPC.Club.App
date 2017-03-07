using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class TransactionFullArticle
    {
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public List<FullArticle> Articles { get; set; }
        [DataMember]
        public decimal Cash { get; set; }
        [DataMember]
        public decimal BankCard { get; set; }
    }
}
