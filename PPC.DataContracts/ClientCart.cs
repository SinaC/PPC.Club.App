using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class ClientCart
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public List<SoldArticle> Articles { get; set; }
        [DataMember]
        public bool IsPaid { get; set; }
        [DataMember]
        public bool IsCash { get; set; }
    }
}
