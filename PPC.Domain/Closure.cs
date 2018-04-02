using System;
using System.Runtime.Serialization;
using System.Text;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class Closure
    {
        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public CashRegisterClosure CashRegisterClosure { get; set; }

        [DataMember]
        public string Notes { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Cloture de la caisse du club (date: {CreationTime:F})");
            sb.AppendLine("**********************");
            if (!string.IsNullOrWhiteSpace(Notes))
            {
                sb.AppendLine("Remarques:");
                sb.AppendLine(Notes);
                sb.AppendLine("**********************");
            }
            sb.Append(CashRegisterClosure.ToString());
            //
            return sb.ToString();
        }
    }
}
