using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
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
