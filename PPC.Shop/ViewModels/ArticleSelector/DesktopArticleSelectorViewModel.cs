using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Popups;

namespace PPC.Shop.ViewModels.ArticleSelector
{
    public class DesktopArticleSelectorViewModel : ObservableObject, IArticleSelector
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        public IEnumerable<Article> Articles => IocContainer.Default.Resolve<IArticleDb>().GetArticles(SelectedCategory);
        public IEnumerable<string> Categories => IocContainer.Default.Resolve<IArticleDb>().Categories;
        public IEnumerable<string> Producers => IocContainer.Default.Resolve<IArticleDb>().Producers;
        private Func<string, IEnumerable<string>> BuildSubCategories => IocContainer.Default.Resolve<IArticleDb>().SubCategories;

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
                            Article article = IocContainer.Default.Resolve<IArticleDb>().GetByEan(_ean);
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
                        ? (int?)null
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
            ArticleSelected?.Invoke(this, new ArticleSelectedEventArgs(SelectedArticle, Quantity.Value));
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
                IocContainer.Default.Resolve<IArticleDb>().Add(article);
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            SelectedCategory = null;

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => Producers);

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

            try
            {
                IocContainer.Default.Resolve<IArticleDb>().Save();
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => Producers);
        }

        #endregion

        public void GotFocus()
        {
            IsArticleNameFocused = true; // grrrrrrrrr f**king focus
        }

        #region IArticleSelector

        public event EventHandler<ArticleSelectedEventArgs> ArticleSelected;

        #endregion
    }

    public class DesktopArticleSelectorViewModelDesignData : DesktopArticleSelectorViewModel
    {
        public DesktopArticleSelectorViewModelDesignData()
        {
            IocContainer.Default.Unregister<IArticleDb>();
            IocContainer.Default.RegisterInstance<IArticleDb>(new ArticlesDesignData(new List<Article>
            {
                new Article
                {
                    Ean = "1111111",
                    Description = "Article1",
                    Price = 10,
                    Category = "Cat1",
                    SubCategory = "SubCategory11"
                },
                new Article
                {
                    Ean = "2222222",
                    Description = "AAAAAAAAAA AAAAA AAAAAA AAAAA Article2",
                    Price = 10,
                    Category = "Cat1",
                    SubCategory = "SubCategory11"
                },
                new Article
                {
                    Ean = "3333333",
                    Description = "Article3",
                    Price = 10,
                    Category = "Cat2",
                    SubCategory = "Sub21"
                },
                new Article
                {
                    Ean = "4444444",
                    Description = "Article4",
                    Price = 10,
                    Category = "Cat3",
                    SubCategory = "Sub31"
                },
            }));

            SelectedArticle = IocContainer.Default.Resolve<IArticleDb>().Articles.FirstOrDefault();
        }
    }
}
