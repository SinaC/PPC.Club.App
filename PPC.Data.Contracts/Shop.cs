using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class Shop
    {
        [DataMember]
        public List<ShopTransaction> Transactions { get; set; }
    }
}
