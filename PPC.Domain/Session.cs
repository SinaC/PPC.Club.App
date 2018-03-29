using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class Session
    {
        [BsonId]
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public DateTime? ClosingTime { get; set; }

        [DataMember]
        public DateTime? LastReloadTime { get; set; }

        [DataMember]
        public List<ShopTransaction> Transactions { get; set; }

        [DataMember]
        public List<ClientCart> ClientCarts { get; set; }
    }
}
