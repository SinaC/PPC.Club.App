using PPC.MVVM;

namespace PPC.Sale
{
    public class ShoppingCartArticleItem : ObservableObject
    {
        public Article Article { get; set; }

        private int _quantity;

        public int Quantity
        {
            get {  return _quantity;}
            set
            {
                if (Set(() => Quantity, ref _quantity, value))
                    RaisePropertyChanged(() => Total);
            }
        }

        public double Total => Quantity*Article.Price;
    }
}
