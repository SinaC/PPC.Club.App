using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class ShopTransaction
    {
        [BsonId]
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public List<Item> Articles { get; set; }

        [DataMember]
        public decimal Cash { get; set; }

        [DataMember]
        public decimal BankCard { get; set; }

        [DataMember]
        public decimal DiscountPercentage { get; set; } // (Cash+BankCard)/(1-DiscountPercentage) = Articles.Sum(x => x.Price*x.Quantity)
    }
}
