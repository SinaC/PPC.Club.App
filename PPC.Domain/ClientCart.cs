using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class ClientCart
    {
        [BsonId]
        [DataMember]
        public Guid Guid { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string ClientFirstName { get; set; }

        [DataMember]
        public string ClientLastName { get; set; }

        [DataMember]
        public string DciNumber { get; set; }

        [DataMember]
        public bool HasFullPlayerInfos { get; set; }

        [DataMember]
        public List<Item> Articles { get; set; }

        [DataMember]
        public bool IsPaid { get; set; }

        [DataMember]
        public decimal Cash { get; set; }

        [DataMember]
        public decimal BankCard { get; set; }

        [DataMember]
        public DateTime PaymentTimeStamp { get; set; }
    }
}
