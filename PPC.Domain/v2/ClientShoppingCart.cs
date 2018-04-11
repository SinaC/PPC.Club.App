using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
{
    [DataContract(Namespace = "")]
    public class ClientShoppingCart
    {
        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public Transaction Transaction { get; set; } // set if cart is paid

        [DataMember]
        public List<ShopArticle> Articles { get; set; }
    }
}
