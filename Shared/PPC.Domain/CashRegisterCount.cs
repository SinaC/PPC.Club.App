using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class CashRegisterCount
    {
        [DataMember]
        public List<CashRegisterCountEntry> Entries { get; set; }

        public int GetCount(decimal value)
        {
            CashRegisterCountEntry entry = Entries?.FirstOrDefault(x => x.Value == value);
            return entry?.Count ?? 0;
        }
    }
}
