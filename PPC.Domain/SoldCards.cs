using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
    public class SoldCards
    {
        [DataMember]
        public string SellerName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public List<SoldCard> Cards { get; set; }

        public decimal Total => Cards?.Sum(x => x.Total) ?? 0;

        public SoldCards()
        {
            Cards = new List<SoldCard>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Vente de carte à la pièce au club (date: {DateTime.Now:F})");
            sb.AppendLine("**********************");
            sb.AppendLine($"Total: {Total:C}");
            sb.AppendLine("**********************");
            foreach (SoldCard soldCard in Cards)
                sb.AppendLine($"{soldCard.CardName} at {soldCard.Price:C} * {soldCard.Quantity} = {soldCard.Total:C}");
            return sb.ToString();
        }
    }
}
