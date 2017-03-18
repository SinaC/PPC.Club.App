using System;
using System.Linq;
using EasyMVVM;
using PPC.Shop.Models;

namespace PPC.Shop.ViewModels
{
    public class CashRegisterViewModel : ObservableObject
    {
        private readonly Action<ShopTransactionItem> _addTransactionAction;

        private ShoppingCartViewModel _shoppingCart;
        public ShoppingCartViewModel ShoppingCart
        {
            get { return _shoppingCart; }
            protected set { Set(() => ShoppingCart, ref _shoppingCart, value); }
        }

        public CashRegisterViewModel(Action<ShopTransactionItem> addTransactionAction)
        {
            _addTransactionAction = addTransactionAction;

            ShoppingCart = new ShoppingCartViewModel(Payment);
        }

        private void Payment(decimal cash, decimal bankCard)
        {
            // Create shoptransaction from shopping cart
            ShopTransactionItem transaction = new ShopTransactionItem
            {
                Timestamp = DateTime.Now,
                Articles = ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
                {
                    Article = x.Article,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = cash,
                BankCard = bankCard
            };

            // Clear shopping cart
            ShoppingCart.Clear();

            // Inform about transaction
            _addTransactionAction(transaction);
        }
    }

    public class CashRegisterViewModelDesignData : CashRegisterViewModel
    {
        public CashRegisterViewModelDesignData() :
            base(t => { })
        {
            ShoppingCart = new ShoppingCartViewModelDesignData();
        }
    }
}
