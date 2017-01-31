using PPC.Tab;

namespace PPC.Sale
{
    public abstract class ShoppingCartTabBase : TabBase
    {
        private bool _isPaid;
        public bool IsPaid {
            get { return _isPaid; }
            set { Set(() => IsPaid, ref _isPaid, value); }
        }

        private ShoppingCartViewModel _shoppingCart;
        public ShoppingCartViewModel ShoppingCart
        {
            get { return _shoppingCart; }
            protected set { Set(() => ShoppingCart, ref _shoppingCart, value); }
        }

        private double _cash;
        public double Cash
        {
            get { return _cash; }
            set { Set(() => Cash, ref _cash, value); }
        }

        private double _bankCard;
        public double BankCard
        {
            get { return _bankCard; }
            set { Set(() => BankCard, ref _bankCard, value); }
        }
    }
}
