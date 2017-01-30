using PPC.MVVM;

namespace PPC.Sale
{
    public class SoldArticleItem : ObservableObject
    {
        public Article Article { get; set; }

        private int _quantityCash;
        public int QuantityCash
        {
            get { return _quantityCash; }
            set
            {
                if (Set(() => QuantityCash, ref _quantityCash, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        private int _quantityBankCard;
        public int QuantityBankCard
        {
            get { return _quantityBankCard; }
            set
            {
                if (Set(() => QuantityBankCard, ref _quantityBankCard, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        public double Total => (QuantityCash + QuantityBankCard) * Article.Price;
        public double TotalBankCard => QuantityBankCard * Article.Price;
        public double TotalCash => QuantityCash * Article.Price;
    }
}
