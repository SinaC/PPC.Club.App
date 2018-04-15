using System;
using System.Linq;
using EasyIoc;
using EasyMVVM;
using PPC.Log;
using PPC.Module.Shop.Models;

namespace PPC.Module.Shop.ViewModels
{
    public class CashRegisterViewModel : ObservableObject
    {
        private ILog Logger => IocContainer.Default.Resolve<ILog>();

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

        private void Payment(decimal cash, decimal bankCard, decimal discountPercentage)
        {
            Logger.Info($"Payment. Cash:{cash:C} BankCard:{bankCard:C} Discount:{discountPercentage}%");

            // Create shoptransaction from shopping cart
            ShopTransactionItem transaction = new ShopTransactionItem
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Articles = ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
                {
                    Article = x.Article,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = cash,
                BankCard = bankCard,
                DiscountPercentage = discountPercentage
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
