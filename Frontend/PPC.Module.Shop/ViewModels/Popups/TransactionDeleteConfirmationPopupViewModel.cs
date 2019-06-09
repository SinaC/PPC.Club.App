using System;
using System.Collections.Generic;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Domain;
using PPC.Module.Shop.Models;
using PPC.Module.Shop.Views.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels.Popups
{
    [PopupAssociatedView(typeof(TransactionDeleteConfirmationPopup))]
    public class TransactionDeleteConfirmationPopupViewModel : ObservableObject
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private readonly Action<ShopTransactionItem> _confirmDeleteAction;
        private readonly ShopTransactionItem _transactionItem;

        public IEnumerable<ShopArticleItem> Articles => _transactionItem.Articles;
        public decimal Cash => _transactionItem.Cash;
        public decimal BankCard => _transactionItem.BankCard;
        public decimal DiscountPercentage => _transactionItem.DiscountPercentage;

        private ICommand _confirmCommand;
        public ICommand ConfirmCommand => _confirmCommand = _confirmCommand ?? new RelayCommand(Confirm);
        private void Confirm()
        {
            _confirmDeleteAction?.Invoke(_transactionItem);
            PopupService?.Close(this);
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand => _cancelCommand = _cancelCommand ?? new RelayCommand(Cancel);
        private void Cancel()
        {
            PopupService?.Close(this);
        }

        public TransactionDeleteConfirmationPopupViewModel(ShopTransactionItem transactionItem, Action<ShopTransactionItem> confirmDeleteAction)
        {
            _transactionItem = transactionItem;
            _confirmDeleteAction = confirmDeleteAction;

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Cash);
            RaisePropertyChanged(() => BankCard);
            RaisePropertyChanged(() => DiscountPercentage);
        }
    }

    public class TransactionDeleteConfirmationPopupViewModelDesignData : TransactionDeleteConfirmationPopupViewModel
    {
        public TransactionDeleteConfirmationPopupViewModelDesignData() : base(new ShopTransactionItem
        {
            Cash = 94,
            BankCard = 5,
            DiscountPercentage = 0.10m,
            Articles = new List<ShopArticleItem>
            {
                new ShopArticleItem
                {
                    Article = new Article
                    {
                        Ean = "1111111",
                        Description = "Article1",
                        Price = 10
                    },
                    Quantity = 2,
                },
                new ShopArticleItem
                {
                    Article = new Article
                    {
                        Ean = "222222222",
                        Description = "Article2",
                        Price = 20
                    },
                    Quantity = 3,
                },
                new ShopArticleItem
                {
                    Article = new Article
                    {
                        Ean = "33333333",
                        Description = "Article3",
                        Price = 30
                    },
                    Quantity = 1,
                }
            }
        }, _ => { })
        {
        }
    }
}
