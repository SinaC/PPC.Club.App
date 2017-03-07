using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public class CashRegisterClosureConfig
    {
        [DataMember]
        public string SenderMail { get; set; }

        [DataMember]
        public string SenderPassword { get; set; }

        [DataMember]
        public string RecipientMail { get; set; }
    }
}
