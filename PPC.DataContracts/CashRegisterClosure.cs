using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PPC.DataContracts
{
    [DataContract]
    public class CashRegisterClosure
    {
        [DataMember]
        public List<FullArticle> Articles { get; set; }
        [DataMember]
        public decimal Cash { get; set; }
        [DataMember]
        public decimal BankCard { get; set; }
        [DataMember]
        public List<TransactionFullArticle> Transactions { get; set; }

        public decimal Total => Cash + BankCard;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Cloture de la caisse du club (date: {DateTime.Now:F})");
            sb.AppendLine("**********************");
            sb.AppendLine("Résumé:");
            sb.AppendLine("===================");
            sb.AppendLine($"Cash: {Cash:C}");
            sb.AppendLine($"Bancontact: {BankCard:C}");
            sb.AppendLine("Articles:");
            if (Articles != null)
            {
                foreach (FullArticle article in Articles)
                    sb.AppendLine($"{article.Quantity,5} * {article.Ean,15} {article.Description,30} {article.Price,10:C}");
            }
            else
                sb.AppendLine("néant");
            sb.AppendLine("**********************");
            sb.AppendLine("Détails:");
            sb.AppendLine("===================");
            if (Transactions != null)
            {
                int id = 1;
                foreach (TransactionFullArticle transaction in Transactions.OrderBy(x => x.Timestamp))
                {
                    sb.AppendLine($"Transaction:{id}");
                    sb.AppendLine($"Cash:{transaction.Cash:C}");
                    sb.AppendLine($"Bancontact:{transaction.BankCard:C}");
                    sb.AppendLine("Articles:");
                    foreach (FullArticle article in transaction.Articles)
                        sb.AppendLine($"{article.Quantity,5} * {article.Ean,15} {article.Description,30} {article.Price,10:C}");
                    id++;
                    sb.AppendLine("------");
                }
            }
            else
                sb.AppendLine("néant");

            return sb.ToString();
        }
    }
}
