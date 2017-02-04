using PPC.MVVM;

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
