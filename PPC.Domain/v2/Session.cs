using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain.v2
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
        public List<Transaction> Transactions { get; set; }

        [DataMember]
        public List<ClientShoppingCart> ClientCarts { get; set; }

        [DataMember]
        public string Notes { get; set; }
    }
}
