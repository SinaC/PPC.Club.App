using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
{
    [DataContract(Namespace = "")]
    public class Discount
    {
        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public DiscountTypes Type { get; set; }

        [DataMember]
        public decimal? Value { get; set; } // Can be %age or fixed (depends on Type)
    }
}
