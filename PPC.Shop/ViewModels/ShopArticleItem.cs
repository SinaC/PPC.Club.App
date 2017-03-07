using EasyMVVM;
using PPC.DataContracts;

namespace PPC.Shop.ViewModels
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

        public decimal Total => Quantity * Article.Price;
    }
}
