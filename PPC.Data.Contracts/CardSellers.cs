using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
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
