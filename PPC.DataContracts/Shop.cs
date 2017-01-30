using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class Shop
    {
        [DataMember]
        public List<SoldArticle> Articles { get; set; }
    }
}
