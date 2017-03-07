using System;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class Item
    {
        [DataMember]
        public Guid Guid { get; set; }
        [DataMember]
        public int Quantity { get; set; }
    }
}
