using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class CardSellers
    {
        [DataMember]
        public List<CardSeller> Sellers { get; set; }

        public CardSellers()
        {
            Sellers = new List<CardSeller>();
        }
    }
}
