using System;
using System.Collections.Generic;
using PPC.MVVM;

namespace PPC.Sale.ViewModels
{
    public class ShopTransactionItem : ObservableObject
    {
        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get {  return _timestamp;}
            set { Set(() => Timestamp, ref _timestamp, value); }
        }

        private List<ShopArticleItem> _articles;
        public List<ShopArticleItem> Articles
        {
            get { return _articles; }
            set { Set(() => Articles, ref _articles, value); }
        }

        private decimal _cash;
        public decimal Cash
        {
            get { return _cash; }
            set { Set(() => Cash, ref _cash, value); }
        }

        private decimal _bankCard;
        public decimal BankCard
        {
            get { return _bankCard; }
            set { Set(() => BankCard, ref _bankCard, value); }
        }
    }
}
