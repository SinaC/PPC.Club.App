using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Popups;

namespace PPC.Shop.ViewModels
{
    public class ShoppingCartViewModel : ObservableObject
    {
        private static readonly decimal MinBankCard = 5; // TODO: Config ?

        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private static readonly string[] EmptyList = {string.Empty};

        private readonly Action<decimal, decimal> _paymentAction;
        private readonly Action _cartModifiedAction;

        public IEnumerable<Article> Articles => (string.IsNullOrWhiteSpace(SelectedCategory)
            ? ArticlesDb.Instance.Articles
            : ArticlesDb.Instance.Articles.Where(x => x.Category == SelectedCategory)).OrderBy(x => x.Description);

        public IEnumerable<string> Categories => EmptyList.Concat(ArticlesDb.Instance.Articles.Where(x => !string.IsNullOrWhiteSpace(x.Category)).Select(x => x.Category).Distinct());
        public IEnumerable<string> Producers => EmptyList.Concat(ArticlesDb.Instance.Articles.Where(x => !string.IsNullOrWhiteSpace(x.Producer)).Select(x => x.Producer).Distinct());
        private Func<string, IEnumerable<string>> BuildSubCategories => category => EmptyList.Concat(ArticlesDb.Instance.Articles.Where(x => x.Category == category && !string.IsNullOrWhiteSpace(x.SubCategory)).Select(x => x.SubCategory).Distinct());

        #region Cart articles

        public ObservableCollection<ShopArticleItem> ShoppingCartArticles { get; }
        public decimal Total => ShoppingCartArticles.Sum(x => x.Total);

        #endregion

        #region Ean

        private string _ean;

        public string Ean
        {
            get { return _ean; }
            set
            {
                if (Set(() => Ean, ref _ean, value))
                {
                    if (_ean != null)
                    {
                        if (_ean.Length == 13)
                        {
                            Article article = ArticlesDb.Instance.Articles.FirstOrDefault(x => x.Ean == _ean);
                            if (article != null)
                                SelectedArticle = article;
                            else
                                DisplayCreateArticlePopup();
                        }
                        else
                            SelectedArticle = null;
                    }
                }
            }
        }

        #endregion

        #region Article selection

        #region Selected article/category

        private string _selectedCategory;

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (Set(() => SelectedCategory, ref _selectedCategory, value))
                    RaisePropertyChanged(() => Articles);
            }
        }

        private Article _selectedArticle;

        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set
            {
                if (Set(() => SelectedArticle, ref _selectedArticle, value))
                {
                    Quantity = _selectedArticle == null
                        ? (int?) null
                        : 1;
                    _ean = _selectedArticle?.Ean; // set value without retriggering article search
                    RaisePropertyChanged(() => Ean);
                }
            }
        }

        #endregion

        #region Article name filter

        private bool _isArticleNameFocused;

        public bool IsArticleNameFocused
        {
            get { return _isArticleNameFocused; }
            set
            {
                // Force RaisePropertyChanged
                _isArticleNameFocused = value;
                RaisePropertyChanged(() => IsArticleNameFocused);
            }
        }

        public AutoCompleteFilterPredicate<object> ArticleFilterPredicate => FilterArticle;

        private bool FilterArticle(string search, object o)
        {
            if (string.IsNullOrWhiteSpace(search))
                return true;
            Article article = o as Article;
            if (article == null)
                return false;
            //http://stackoverflow.com/questions/359827/ignoring-accented-letters-in-string-comparison/7720903#7720903
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(article.Description, search, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) >= 0;
        }

        #endregion

        #endregion

        #region Quantity

        private int? _quantity;

        public int? Quantity
        {
            get { return _quantity; }
            set { Set(() => Quantity, ref _quantity, value); }
        }

        #endregion

        #region Increment selected article

        private ICommand _incrementSelectedArticleCommand;
        public ICommand IncrementSelectedArticleCommand => _incrementSelectedArticleCommand = _incrementSelectedArticleCommand ?? new RelayCommand(IncrementSelectedArticle, () => SelectedArticle != null);

        private void IncrementSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (!Quantity.HasValue)
                Quantity = 1;
            else
                Quantity++;
        }

        #endregion

        #region Decrement selected article

        private ICommand _decrementSelectedArticleCommand;
        public ICommand DecrementSelectedArticleCommand => _decrementSelectedArticleCommand = _decrementSelectedArticleCommand ?? new RelayCommand(DecrementSelectedArticle, () => SelectedArticle != null);

        private void DecrementSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (Quantity.HasValue)
            {
                if (Quantity.Value == 1)
                    Quantity = null;
                else
                    Quantity--;
            }
        }

        #endregion

        #region Add selected article in cart

        private ICommand _addSelectedArticleCommand;
        public ICommand AddSelectedArticleCommand => _addSelectedArticleCommand = _addSelectedArticleCommand ?? new RelayCommand(AddSelectedArticle, () => SelectedArticle != null);

        private void AddSelectedArticle()
        {
            if (SelectedArticle == null)
                return;
            if (!Quantity.HasValue || Quantity.Value == 0)
                return;
            ShopArticleItem article = ShoppingCartArticles.FirstOrDefault(x => x.Article.Guid == SelectedArticle.Guid);
            if (article == null)
            {
                article = new ShopArticleItem
                {
                    Article = SelectedArticle,
                    Quantity = 0
                };
                ShoppingCartArticles.Add(article);
            }
            article.Quantity += Quantity.Value;
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

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
            PaymentPopupViewModel vm = new PaymentPopupViewModel(PopupService, Total, isCashFirst, _paymentAction);
            PopupService.DisplayModal(vm, "Payment");
        }

        #endregion

        #region Article creation

        private ICommand _createArticleCommand;
        public ICommand CreateArticleCommand => _createArticleCommand = _createArticleCommand ?? new RelayCommand(DisplayCreateArticlePopup);

        private void DisplayCreateArticlePopup()
        {
            SelectedArticle = null;
            CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(PopupService, Categories, Producers, BuildSubCategories, CreateArticle)
            {
                IsEdition = false
            };
            PopupService.DisplayModal(vm, "New article");
        }

        private void CreateArticle(CreateEditArticlePopupViewModel vm)
        {
            Article article = new Article
            {
                Guid = Guid.NewGuid(),
                Ean = vm.Ean,
                Description = vm.Description,
                Category = vm.Category,
                SubCategory = vm.SubCategory,
                Producer = vm.Producer,
                SupplierPrice = vm.SupplierPrice,
                Price = vm.Price,
                Stock = vm.Stock,
                VatRate = vm.VatRate,
                IsNewArticle = true,
            };

            try
            {
                ArticlesDb.Instance.Add(article);
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            SelectedCategory = null;

            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => Producers);
            RaisePropertyChanged(() => Articles);

            SelectedArticle = Articles.FirstOrDefault(x => x.Guid == article.Guid);
            IsArticleNameFocused = true;
        }

        #endregion

        #region Article edition

        private ICommand _editArticleCommand;
        public ICommand EditArticleCommand => _editArticleCommand = _editArticleCommand ?? new RelayCommand(DisplayEditArticlePopup, () => SelectedArticle != null);

        private void DisplayEditArticlePopup()
        {
            if (SelectedArticle != null)
            {
                CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(PopupService, Categories, Producers, BuildSubCategories, SaveArticle)
                {
                    IsEdition = true,
                    Ean = SelectedArticle.Ean,
                    Description = SelectedArticle.Description,
                    Category = SelectedArticle.Category,
                    SubCategory = SelectedArticle.SubCategory,
                    Producer = SelectedArticle.Producer,
                    SupplierPrice = SelectedArticle.SupplierPrice,
                    Price = SelectedArticle.Price,
                    VatRate = SelectedArticle.VatRate,
                    Stock = SelectedArticle.Stock
                };
                PopupService.DisplayModal(vm, "Edit article");
            }
        }

        private void SaveArticle(CreateEditArticlePopupViewModel vm)
        {
            SelectedArticle.Ean = vm.Ean;
            SelectedArticle.Description = vm.Description;
            SelectedArticle.Category = vm.Category;
            SelectedArticle.SubCategory = vm.SubCategory;
            SelectedArticle.Producer = vm.Producer;
            SelectedArticle.SupplierPrice = vm.SupplierPrice;
            SelectedArticle.Price = vm.Price;
            SelectedArticle.VatRate = vm.VatRate;
            SelectedArticle.Stock = vm.Stock;

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Categories);

            try
            {
                ArticlesDb.Instance.Save();
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }
        }

        #endregion

        public void Clear()
        {
            ShoppingCartArticles.Clear();
            RaisePropertyChanged(() => Total);
            _cartModifiedAction?.Invoke();
        }

        public ShoppingCartViewModel(Action<decimal, decimal> paymentAction, Action cartModifiedAction = null)
        {
            _paymentAction = paymentAction;
            _cartModifiedAction = cartModifiedAction;

            ShoppingCartArticles = new ObservableCollection<ShopArticleItem>();
        }
    }

    public class ShoppingCartViewModelDesignData : ShoppingCartViewModel
    {
        public ShoppingCartViewModelDesignData() : base((d, d1) => { }, () => { })
        {
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
            SelectedArticle = ShoppingCartArticles[0].Article;
        }
    }
}
