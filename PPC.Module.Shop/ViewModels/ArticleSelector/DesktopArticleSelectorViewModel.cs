using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Domain;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Popups;
using PPC.Services.Popup;

namespace PPC.Module.Shop.ViewModels.ArticleSelector
{
    public class DesktopArticleSelectorViewModel : ObservableObject, IArticleSelector
    {
        private static readonly string[] EmptyList = { string.Empty };

        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private IArticleDL ArticlesDb => IocContainer.Default.Resolve<IArticleDL>();

        private Func<string, IEnumerable<string>> BuildSubCategories => category => EmptyList.Concat(ArticlesDb.SubCategories(category).OrderBy(x => x));
        public IEnumerable<Article> Articles => ArticlesDb.FilterArticles(SelectedCategory).OrderBy(x => x.Description);
        public IEnumerable<string> Categories => EmptyList.Concat(ArticlesDb.Categories).OrderBy(x => x);
        public IEnumerable<string> Producers => EmptyList.Concat(ArticlesDb.Producers).OrderBy(x => x);

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
                            Article article = ArticlesDb.GetByEan(_ean);
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
            CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(Categories, Producers, BuildSubCategories, CreateArticle)
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
                Logger.Info($"New article {article.Description ?? "???"} {article.Price:C} created.");
                ArticlesDb.Insert(article);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving articles DB", ex);
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
                CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(Categories, Producers, BuildSubCategories, SaveArticle)
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
            Article article = SelectedArticle;
            article.Ean = vm.Ean;
            article.Description = vm.Description;
            article.Category = vm.Category;
            article.SubCategory = vm.SubCategory;
            article.Producer = vm.Producer;
            article.SupplierPrice = vm.SupplierPrice;
            article.Price = vm.Price;
            article.VatRate = vm.VatRate;
            article.Stock = vm.Stock;

            try
            {
                Logger.Info($"Article {article.Description ?? "???"} {article.Price:C} edited.");
                ArticlesDb.Update(article);
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving articles DB", ex);
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => Producers);
        }

        #endregion

        #region IArticleSelector

        public event EventHandler<ArticleSelectedEventArgs> ArticleSelected;

        public void GotFocus()
        {
            IsArticleNameFocused = true; // grrrrrrrrr f**king focus
        }

        #endregion
    }

    public class DesktopArticleSelectorViewModelDesignData : DesktopArticleSelectorViewModel
    {
        public DesktopArticleSelectorViewModelDesignData()
        {
            IocContainer.Default.Unregister<IArticleDL>();
            IocContainer.Default.RegisterInstance<IArticleDL>(new DataAccess.DesignMode.ArticleDL(new List<Article>
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

            SelectedArticle = IocContainer.Default.Resolve<IArticleDL>().Articles.FirstOrDefault();
        }
    }
}
