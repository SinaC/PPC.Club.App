using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class Closing
    {
        [DataMember]
        public List<FullArticle> Articles { get; set; }
        [DataMember]
        public double Cash { get; set; }
        [DataMember]
        public double BankCard { get; set; }
    }
}
