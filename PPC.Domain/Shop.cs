using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class Shop
    {
        [DataMember]
        public List<ShopTransaction> Transactions { get; set; }
    }
}
