using PPC.MVVM;

namespace PPC.Sale
{
    public class ShopArticleItem : ObservableObject
    {
        public Article Article { get; set; }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (Set(() => Quantity, ref _quantity, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        private bool _isCash;
        public bool IsCash
        {
            get { return _isCash; }
            set { Set(() => IsCash, ref _isCash, value); }
        }

        public double Total => Quantity * Article.Price;
    }
}
