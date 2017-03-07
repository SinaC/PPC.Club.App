using EasyMVVM;

namespace PPC.Shop.ViewModels
{
    public enum PaymentStates
    {
        Irrelevant,
        Paid,
        Unpaid,
    }

    public abstract class ShoppingCartBasedViewModelBase : ObservableObject
    {
        private PaymentStates _paymentState;
        public PaymentStates PaymentState
        {
            get { return _paymentState; }
            set { Set(() => PaymentState, ref _paymentState, value); }
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
