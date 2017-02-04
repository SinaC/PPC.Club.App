using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class ClosingConfig
    {
        [DataMember]
        public string SenderMail { get; set; }

        [DataMember]
        public string SenderPassword { get; set; }

        [DataMember]
        public string RecipientMail { get; set; }
    }
}
