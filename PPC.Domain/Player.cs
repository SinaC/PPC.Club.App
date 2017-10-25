using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class Player
    {
        [DataMember]
        public string DCINumber { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string MiddleName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string CountryCode { get; set; }

        [DataMember]
        public bool IsJudge { get; set; }
    }
}