using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class CardSeller
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}
