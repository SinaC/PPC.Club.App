using System;
using System.Runtime.Serialization;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class Closure
    {
        [BsonId]
        [DataMember]
        public Guid Guid { get; set; }

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
            sb.Append(CashRegisterClosure);
            //
            return sb.ToString();
        }

        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Cloture de la caisse du club (date: {CreationTime:F})");
            sb.AppendLine("**********************");
            if (!string.IsNullOrWhiteSpace(Notes))
            {
                sb.AppendLine("<font color=\"red\">");
                sb.AppendLine("Remarques:");
                sb.AppendLine(Notes);
                sb.AppendLine("</font>");
                sb.AppendLine("**********************");
            }
            sb.Append(CashRegisterClosure.ToHtml());
            //
            return sb.Replace(Environment.NewLine, "<br/>").ToString();
        }
    }
}
