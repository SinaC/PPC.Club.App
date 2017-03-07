using System;
using System.Linq;

namespace PPC.Shop.ViewModels
{
    public class CashRegisterViewModel : ShoppingCartBasedViewModelBase
    {
        private readonly Action<ShopTransactionItem> _addTransactionAction;

        public CashRegisterViewModel(Action<ShopTransactionItem> addTransactionAction)
        {
            PaymentState = PaymentStates.Irrelevant;

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

            // Add cash/bank card
            Cash += cash;
            BankCard += bankCard;

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
