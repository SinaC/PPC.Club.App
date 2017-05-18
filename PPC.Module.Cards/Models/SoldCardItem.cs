using EasyMVVM;

namespace PPC.Module.Cards.Models
{
    public class SoldCardItem : ObservableObject
    {
        private string _cardName;
        public string CardName
        {
            get { return _cardName; }
            set { Set(() => CardName, ref _cardName, value); }
        }

        private decimal _price;
        public decimal Price
        {
            get { return _price; }
            set
            {
                if (Set(() => Price, ref _price, value))
                    RaisePropertyChanged(() => Total);
            }
        }

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

        public decimal Total => Price*Quantity;
    }
}
