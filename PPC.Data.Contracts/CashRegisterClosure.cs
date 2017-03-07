using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PPC.Data.Contracts
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
            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine($"Cloture de la caisse du club (date: {DateTime.Now:F})");
            //sb.AppendLine("**********************");
            //sb.AppendLine("Résumé:");
            //sb.AppendLine("===================");
            //sb.AppendLine($"Cash: {Cash:C}");
            //sb.AppendLine($"Bancontact: {BankCard:C}");
            //sb.AppendLine("Articles:");
            //if (Articles != null)
            //{
            //    foreach (FullArticle article in Articles)
            //        sb.AppendLine($"{article.Quantity,5} * {article.Ean,15} {article.Description,30} {article.Price,10:C}");
            //}
            //else
            //    sb.AppendLine("néant");
            //sb.AppendLine("**********************");
            //sb.AppendLine("Détails:");
            //sb.AppendLine("===================");
            //if (Transactions != null)
            //{
            //    int id = 1;
            //    foreach (TransactionFullArticle transaction in Transactions.OrderBy(x => x.Timestamp))
            //    {
            //        sb.AppendLine($"Transaction:{id}");
            //        sb.AppendLine($"Cash:{transaction.Cash:C}");
            //        sb.AppendLine($"Bancontact:{transaction.BankCard:C}");
            //        sb.AppendLine("Articles:");
            //        foreach (FullArticle article in transaction.Articles)
            //            sb.AppendLine($"{article.Quantity,5} * {article.Ean,15} {article.Description,30} {article.Price,10:C}");
            //        id++;
            //        sb.AppendLine("------");
            //    }
            //}
            //else
            //    sb.AppendLine("néant");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Cloture de la caisse du club (date: {DateTime.Now:F})");
            sb.AppendLine("**********************");
            sb.AppendLine($"Cash: {Cash:C}");
            sb.AppendLine($"Bancontact: {BankCard:C}");
            sb.AppendLine("**********************");
            // Group cash transactions
            decimal cash = 0;
            Dictionary<Guid, FullArticle> cashArticles = new Dictionary<Guid, FullArticle>();
            foreach (TransactionFullArticle transaction in Transactions.Where(x => x.BankCard == 0))
            {
                cash += transaction.Cash;
                foreach (FullArticle article in transaction.Articles)
                {
                    FullArticle existingArticle;
                    if (!cashArticles.TryGetValue(article.Guid, out existingArticle))
                    {
                        existingArticle = new FullArticle
                        {
                            Guid = article.Guid,
                            Ean = article.Ean,
                            Description = article.Description,
                            Price = article.Price,
                            Quantity = 0,
                        };
                        cashArticles.Add(existingArticle.Guid, existingArticle);
                    }
                    existingArticle.Quantity += article.Quantity;
                }
            }
            sb.AppendLine($"Cash: {cash:C}");
            sb.AppendLine("Articles:");
            foreach (FullArticle article in cashArticles.Values)
                sb.AppendLine(article.ToString());
            sb.AppendLine("**********************");
            // Transactions with positive bank payment
            foreach (TransactionFullArticle transaction in Transactions.Where(x => x.BankCard > 0))
            {
                sb.AppendLine($"Cash: {transaction.Cash:C}");
                sb.AppendLine($"Bancontact: {transaction.BankCard:C}");
                sb.AppendLine("Articles:");
                foreach (FullArticle article in transaction.Articles)
                    sb.AppendLine(article.ToString());
                sb.AppendLine("===================");
            }
            //
            return sb.ToString();
        }
    }
}
