using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Module.Shop.Models;
using PPC.Module.Shop.ViewModels.ArticleSelector;
using PPC.Module.Shop.ViewModels.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels
{
    public class ShoppingCartViewModel : ObservableObject
    {
        private static readonly decimal MinBankCard = 5; // TODO: Config ?

        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private Action<decimal, decimal, decimal> _paymentAction;
        private Action _cartModifiedAction;

        public IArticleSelector MobileArticleSelector { get; protected set; }
        public IArticleSelector DesktopArticleSelector { get; protected set; }

        private IArticleSelector _currentArticleSelectorViewModel;

        public IArticleSelector CurrentArticleSelectorViewModel
        {
            get { return _currentArticleSelectorViewModel; }
            set
            {
                if (Set(() => CurrentArticleSelectorViewModel, ref _currentArticleSelectorViewModel, value))
                {
                    RaisePropertyChanged(() => IsMobile);
                    RaisePropertyChanged(() => IsDesktop);
                }
            }
        }

        public bool IsMobile => CurrentArticleSelectorViewModel == MobileArticleSelector;
        public bool IsDesktop => CurrentArticleSelectorViewModel == DesktopArticleSelector;

        private ICommand _switchModeCommand;
        public ICommand SwitchModeCommand => _switchModeCommand = _switchModeCommand ?? new RelayCommand(SwitchMode);
        private void SwitchMode()
        {
            CurrentArticleSelectorViewModel = CurrentArticleSelectorViewModel == MobileArticleSelector ? DesktopArticleSelector : MobileArticleSelector;
        }

        #region Cart articles

        public ObservableCollection<ShopArticleItem> ShoppingCartArticles { get; }
        public decimal Total => ShoppingCartArticles.Sum(x => x.Total);

        #endregion

        #region Delete article from cart

        private ICommand _deleteArticleCommand;
        public ICommand DeleteArticleCommand => _deleteArticleCommand = _deleteArticleCommand ?? new RelayCommand<ShopArticleItem>(DeleteArticle);

        private void DeleteArticle(ShopArticleItem item)
        {
            ShoppingCartArticles.Remove(item);
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        #endregion

        #region Increment article in cart

        private ICommand _incrementArticleCommand;
        public ICommand IncrementArticleCommand => _incrementArticleCommand = _incrementArticleCommand ?? new RelayCommand<ShopArticleItem>(IncrementArticle);

        private void IncrementArticle(ShopArticleItem item)
        {
            item.Quantity++;
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        #endregion

        #region Decrement article in cart

        private ICommand _decrementArticleCommand;
        public ICommand DecrementArticleCommand => _decrementArticleCommand = _decrementArticleCommand ?? new RelayCommand<ShopArticleItem>(DecrementArticle);

        private void DecrementArticle(ShopArticleItem item)
        {
            if (item.Quantity == 1)
                DeleteArticle(item);
            else
            {
                item.Quantity--;
                RaisePropertyChanged(() => Total);
            }
            _cartModifiedAction?.Invoke();
        }

        #endregion

        // TODO: mark article as free (discount)

        #region Payment

        #region Cash payment

        private ICommand _cashCommand;
        public ICommand CashCommand => _cashCommand = _cashCommand ?? new RelayCommand(() => DisplayPaymentPopup(true), () => ShoppingCartArticles.Any());

        #endregion

        #region Bank card payment

        private ICommand _bankCardCommand;
        public ICommand BankCardCommand => _bankCardCommand = _bankCardCommand ?? new RelayCommand(() => DisplayPaymentPopup(false), () => ShoppingCartArticles.Any() && Total >= MinBankCard);

        #endregion

        private void DisplayPaymentPopup(bool isCashFirst)
        {
            PaymentPopupViewModel vm = new PaymentPopupViewModel(Total, isCashFirst, _paymentAction);
            PopupService.DisplayModal(vm, "Payment");
        }

        #endregion

        public void GotFocus()
        {
            CurrentArticleSelectorViewModel.GotFocus();
        }

        public void Clear()
        {
            ShoppingCartArticles.Clear();
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        public void RemoveHandlers()
        {
            MobileArticleSelector.ArticleSelected -= AddArticle;
            DesktopArticleSelector.ArticleSelected -= AddArticle;

            _paymentAction = null;
            _cartModifiedAction = null;
        }

        public ShoppingCartViewModel(Action<decimal, decimal, decimal> paymentAction, Action cartModifiedAction = null)
        {
            MobileArticleSelector = new MobileArticleSelectorViewModel();
            MobileArticleSelector.ArticleSelected += AddArticle;
            DesktopArticleSelector = new DesktopArticleSelectorViewModel();
            DesktopArticleSelector.ArticleSelected += AddArticle;

            CurrentArticleSelectorViewModel = DesktopArticleSelector;


            _paymentAction = paymentAction;
            _cartModifiedAction = cartModifiedAction;

            ShoppingCartArticles = new ObservableCollection<ShopArticleItem>();
        }

        private void AddArticle(object sender, ArticleSelectedEventArgs args)
        {
            ShopArticleItem article = ShoppingCartArticles.FirstOrDefault(x => x.Article.Guid == args.Article.Guid);
            if (article == null)
            {
                article = new ShopArticleItem
                {
                    Article = args.Article,
                    Quantity = 0
                };
                ShoppingCartArticles.Add(article);
            }
            article.Quantity += args.Quantity;
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }
    }

    public class ShoppingCartViewModelDesignData : ShoppingCartViewModel
    {
        public ShoppingCartViewModelDesignData() : base((a, b, c) => { }, () => { })
        {
            MobileArticleSelector = new MobileArticleSelectorViewModelDesignData();
            DesktopArticleSelector = new DesktopArticleSelectorViewModelDesignData();

            CurrentArticleSelectorViewModel = MobileArticleSelector;

            ShoppingCartArticles.Clear();
            ShoppingCartArticles.AddRange(new[]
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
            });
            ShoppingCartArticles.AddRange(Enumerable.Range(0, 100).Select(x => new ShopArticleItem
            {
                Article = new Article
                {
                    Ean = "1111111",
                    Description = "Article1",
                    Price = 10 + x
                },
                Quantity = x + 1,
            }));
        }
    }
}
