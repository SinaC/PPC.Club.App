using System;
using System.Runtime.Serialization;

namespace PPC.DataContracts
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
