using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PPC.Domain
{
    [DataContract(Namespace = "")]
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
            sb.AppendLine($"Cash: {Cash:C}");
            sb.AppendLine($"Bancontact: {BankCard:C}");
            sb.AppendLine("**********************");
            if (Transactions.Any(x => x.BankCard == 0 && x.DiscountPercentage == 0))
            {
                // Group cash-only transactions
                decimal cash = 0;
                Dictionary<Guid, FullArticle> cashArticles = new Dictionary<Guid, FullArticle>();
                foreach (TransactionFullArticle transaction in Transactions.Where(x => x.BankCard == 0 && x.DiscountPercentage == 0))
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
            }
            // Transactions with positive bank payment or non-zero discount
            foreach (TransactionFullArticle transaction in Transactions.Where(x => x.BankCard > 0 || x.DiscountPercentage > 0))
            {
                sb.AppendLine($"Cash: {transaction.Cash:C}");
                sb.AppendLine($"Bancontact: {transaction.BankCard:C}");
                if (transaction.DiscountPercentage > 0)
                    sb.AppendLine($"Remise: {transaction.DiscountPercentage:P}");
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
