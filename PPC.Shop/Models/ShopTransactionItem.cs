using System;
using System.Collections.Generic;
using EasyMVVM;

namespace PPC.Shop.Models
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
            set
            {
                if (Set(() => Cash, ref _cash, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        private decimal _bankCard;
        public decimal BankCard
        {
            get { return _bankCard; }
            set
            {
                if (Set(() => BankCard, ref _bankCard, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        public decimal Total => Cash + BankCard;
    }
}
