using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
{
    [DataContract(Namespace = "")]
    public class Closure
    {
        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public List<Article> Articles { get; set; }

        [DataMember]
        public decimal Cash { get; set; }

        [DataMember]
        public decimal BankCard { get; set; }

        [DataMember]
        public List<Transaction> Transactions { get; set; }
    }
}
