using System;
using System.Collections.Generic;
using EasyMVVM;

namespace PPC.Module.Shop.Models
{
    public class ShopTransactionItem : ObservableObject
    {
        public Guid Id { get; set; }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get { return _timestamp; }
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
                {
                    RaisePropertyChanged(() => Total);
                    RaisePropertyChanged(() => TotalWithoutDiscount);
                }
            }
        }

        private decimal _bankCard;
        public decimal BankCard
        {
            get { return _bankCard; }
            set
            {
                if (Set(() => BankCard, ref _bankCard, value))
                {
                    RaisePropertyChanged(() => Total);
                    RaisePropertyChanged(() => TotalWithoutDiscount);
                }
            }
        }

        private decimal _discountPercentage;
        public decimal DiscountPercentage
        {
            get { return _discountPercentage; }
            set
            {
                if (Set(() => DiscountPercentage, ref _discountPercentage, value))
                {
                    RaisePropertyChanged(() => Total);
                    RaisePropertyChanged(() => TotalWithoutDiscount);
                }
            }
        }

        public decimal TotalWithoutDiscount => DiscountPercentage == 0 ? Cash + BankCard : (Cash + BankCard) / (1 - DiscountPercentage);
        public decimal Total => Cash + BankCard;
    }
}
