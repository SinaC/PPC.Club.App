using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
{
    [DataContract(Namespace = "")]
    public class Transaction
    {
        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public TransactionTypes Type { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }

        [DataMember]
        public DateTime? UpdatedOn { get; set; }

        [DataMember]
        public List<ShopArticle> Articles { get; set; }

        [DataMember]
        public decimal Total { get; set; } // Without discount

        [DataMember]
        public decimal Cash { get; set; } // Can be greater than total

        [DataMember]
        public decimal BankCard { get; set; } // Should not be greater than total

        [DataMember]
        public Discount Discount { get; set; }

        [DataMember]
        public Transaction TransactionBeforeModification { get; set; }
    }
}
