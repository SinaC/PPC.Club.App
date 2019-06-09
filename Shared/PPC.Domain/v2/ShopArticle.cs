using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
{
    [DataContract(Namespace = "")]
    public class ShopArticle
    {
        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Article Article { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public Discount Discount { get; set; }
    }
}
