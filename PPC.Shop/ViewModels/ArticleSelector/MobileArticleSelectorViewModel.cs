using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Popups;

namespace PPC.Shop.ViewModels.ArticleSelector
{
    public enum ArticleSelectorModes
    {
        CategorySelection,
        SubCategorySelection,
        ArticleSelection
    }

    public class MobileArticleSelectorViewModel : ObservableObject, IArticleSelector
    {
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();

        private Func<string, IEnumerable<string>> BuildSubCategories => IocContainer.Default.Resolve<IArticleDb>().SubCategories;

        public IEnumerable<string> Categories => IocContainer.Default.Resolve<IArticleDb>().Categories;
        public IEnumerable<string> Producers => IocContainer.Default.Resolve<IArticleDb>().Producers;

        public IEnumerable<string> SubCategories => BuildSubCategories(SelectedSubCategory);

        public IEnumerable<Article> Articles => IocContainer.Default.Resolve<IArticleDb>().GetArticles(SelectedCategory, SelectedSubCategory);

        public string CurrentSelectionPath => SelectedCategory.AppendIfNotEmpty(">").AppendIfNotEmpty(SelectedSubCategory.AppendIfNotEmpty(">")).AppendIfNotEmpty((SelectedArticle?.Description).AppendIfNotEmpty("(").AppendIfNotEmpty(SelectedArticle?.Price.ToString("C")).AppendIfNotEmpty(")"));

        private IEnumerable<object> _currentlyDisplayedCollection;
        public IEnumerable<object> CurrentlyDisplayedCollection
        {
            get { return _currentlyDisplayedCollection;}
            protected set
            {
                if (Set(() => CurrentlyDisplayedCollection, ref _currentlyDisplayedCollection, value))
                    Debug.WriteLine($"Collection: {value?.Count()}");
            }
        }

        #region Back

        private ICommand _backCommand;
        public ICommand BackCommand => _backCommand = _backCommand ?? new RelayCommand(Back, () => Mode == ArticleSelectorModes.ArticleSelection || Mode == ArticleSelectorModes.SubCategorySelection);

        private void Back()
        {
            switch (Mode)
            {
                case ArticleSelectorModes.ArticleSelection:
                    // if no subcategories, back to category selection
                    SelectedArticle = null;
                    Quantity = 1;
                    if (SubCategories.Any())
                    {
                        SelectedSubCategory = null;
                        Mode = ArticleSelectorModes.SubCategorySelection;
                        CurrentlyDisplayedCollection = SubCategories;
                    }
                    else
                    {
                        SelectedCategory = null;
                        Mode = ArticleSelectorModes.CategorySelection;
                        CurrentlyDisplayedCollection = Categories;
                    }
                    break;
                case ArticleSelectorModes.SubCategorySelection:
                    SelectedCategory = null;
                    Mode = ArticleSelectorModes.CategorySelection;
                    CurrentlyDisplayedCollection = Categories;
                    break;
                default:
                    throw new InvalidOperationException($"Cannot use back button in mode {Mode}");
            }
        }

        #endregion

        #region Category/SubCategory/Article selection

        private ICommand _selectCategoryOrSubCategoryCommand;
        public ICommand SelectCategoryOrSubCategoryCommand => _selectCategoryOrSubCategoryCommand = _selectCategoryOrSubCategoryCommand ?? new RelayCommand<string>(SelectCategoryOrSubCategory);

        private void SelectCategoryOrSubCategory(string categoryOrSubCategory)
        {
            if (Mode == ArticleSelectorModes.CategorySelection)
            {
                SelectedCategory = categoryOrSubCategory;
                // if no subcategories, direct article selection
                if (SubCategories.Any())
                {
                    Mode = ArticleSelectorModes.SubCategorySelection;
                    CurrentlyDisplayedCollection = SubCategories;
                }
                else
                {
                    Mode = ArticleSelectorModes.ArticleSelection;
                    CurrentlyDisplayedCollection = Articles;
                }
            }
            else if (Mode == ArticleSelectorModes.SubCategorySelection)
            {
                Mode = ArticleSelectorModes.ArticleSelection;
                SelectedSubCategory = categoryOrSubCategory;
                CurrentlyDisplayedCollection = Articles;
            }
            RaisePropertyChanged(() => CurrentSelectionPath);
        }

        private string _selectedCategory;

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            protected set
            {
                if (Set(() => SelectedCategory, ref _selectedCategory, value))
                {
                    SelectedSubCategory = null;
                    RaisePropertyChanged(() => SubCategories);
                    RaisePropertyChanged(() => CurrentSelectionPath);
                }
            }
        }

        private ICommand _selectCategoryCommand;
        public ICommand SelectCategoryCommand => _selectCategoryCommand = _selectCategoryCommand ?? new RelayCommand<string>(SelectCategory);

        private void SelectCategory(string category)
        {
            SelectedCategory = category;
            // if no subcategories, direct article selection
            if (SubCategories.Any())
            {
                Mode = ArticleSelectorModes.SubCategorySelection;
                CurrentlyDisplayedCollection = SubCategories;
            }
            else
            {
                Mode = ArticleSelectorModes.ArticleSelection;
                CurrentlyDisplayedCollection = Articles;
            }
        }

        private string _selectedSubCategory;

        public string SelectedSubCategory
        {
            get { return _selectedSubCategory; }
            protected set
            {
                if (Set(() => SelectedSubCategory, ref _selectedSubCategory, value))
                {
                    RaisePropertyChanged(() => Articles);
                    RaisePropertyChanged(() => CurrentSelectionPath);
                }
            }
        }

        private ICommand _selectSubCategoryCommand;
        public ICommand SelectSubCategoryCommand => _selectSubCategoryCommand = _selectSubCategoryCommand ?? new RelayCommand<string>(SelectSubCategory);

        private void SelectSubCategory(string subCategory)
        {
            Mode = ArticleSelectorModes.ArticleSelection;
            SelectedSubCategory = subCategory;
            CurrentlyDisplayedCollection = Articles;
        }

        private Article _selectedArticle;

        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            protected set { Set(() => SelectedArticle, ref _selectedArticle, value); }
        }

        private ICommand _selectArticleCommand;
        public ICommand SelectArticleCommand => _selectArticleCommand = _selectArticleCommand ?? new RelayCommand<Article>(SelectArticle);

        private void SelectArticle(Article article)
        {
            SelectedArticle = article;
            Quantity = 1;
            RaisePropertyChanged(() => CurrentSelectionPath);
        }

        #endregion

        #region Quantity

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set { Set(() => Quantity, ref _quantity, value); }
        }

        private ICommand _incrementQuantityCommand;
        public ICommand IncrementQuantityCommand => _incrementQuantityCommand = _incrementQuantityCommand ?? new RelayCommand(IncrementQuantity, () => SelectedArticle != null);

        private void IncrementQuantity()
        {
            Quantity++;
        }

        private ICommand _decrementQuantityCommand;
        public ICommand DecrementQuantityCommand => _decrementQuantityCommand = _decrementQuantityCommand ?? new RelayCommand(DecrementQuantity, () => SelectedArticle != null && Quantity >= 1);

        private void DecrementQuantity()
        {
            Quantity--;
        }

        #endregion

        #region Add article to shopping cart

        private ICommand _addArticleCommand;
        public ICommand AddArticleCommand => _addArticleCommand = _addArticleCommand ?? new RelayCommand(AddArticle, () => SelectedArticle != null);

        private void AddArticle()
        {
            if (SelectedArticle == null)
                return;
            if (Quantity <= 0)
                return;

            ArticleSelected?.Invoke(this, new ArticleSelectedEventArgs(SelectedArticle, Quantity));

            SelectedArticle = null;
            SelectedSubCategory = null;
            SelectedCategory = null;
            Quantity = 1;
            Mode = ArticleSelectorModes.CategorySelection;
            CurrentlyDisplayedCollection = Categories;
        }

        #endregion

        #region Edit article

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

            SelectedArticle = null;
            SelectedSubCategory = null;
            SelectedCategory = null;

            try
            {
                IocContainer.Default.Resolve<IArticleDb>().Save();
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            SelectedCategory = Categories.FirstOrDefault(x => x == article.Category);
            SelectedSubCategory = SubCategories.FirstOrDefault(x => x == article.SubCategory);
            SelectedArticle = Articles.FirstOrDefault(x => x.Guid == article.Guid);

            CurrentlyDisplayedCollection = Articles;
            Mode = ArticleSelectorModes.ArticleSelection;
        }

        #endregion

        #region Create article

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

            SelectedArticle = null;
            SelectedSubCategory = null;
            SelectedCategory = null;

            try
            {
                IocContainer.Default.Resolve<IArticleDb>().Add(article);
            }
            catch (Exception ex)
            {
                PopupService.DisplayError("Error while saving articles DB", ex);
            }

            SelectedCategory = Categories.FirstOrDefault(x => x == article.Category);
            SelectedSubCategory = SubCategories.FirstOrDefault(x => x == article.SubCategory);
            SelectedArticle = Articles.FirstOrDefault(x => x.Guid == article.Guid);

            CurrentlyDisplayedCollection = Articles;
            Mode = ArticleSelectorModes.ArticleSelection;
        }

        #endregion

        #region Mode

        private ArticleSelectorModes _mode;
        public ArticleSelectorModes Mode
        {
            get { return _mode; }
            protected set { Set(() => Mode, ref _mode, value); }
        }

        #endregion

        #region IArticleSelector

        public event EventHandler<ArticleSelectedEventArgs> ArticleSelected;

        #endregion

        public void GotFocus()
        {
            SelectedArticle = null;
            SelectedSubCategory = null;
            SelectedCategory = null;
            Quantity = 1;
            Mode = ArticleSelectorModes.CategorySelection;
            CurrentlyDisplayedCollection = Categories;
        }

        public MobileArticleSelectorViewModel()
        {
            Mode = ArticleSelectorModes.CategorySelection;
            CurrentlyDisplayedCollection = Categories;
        }
    }

    public class MobileArticleSelectorViewModelDesignData : MobileArticleSelectorViewModel
    {
        public MobileArticleSelectorViewModelDesignData()
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

            SelectCategoryCommand.Execute("Cat1");
            SelectSubCategoryCommand.Execute("SubCategory11");

            Mode = ArticleSelectorModes.ArticleSelection;
        }
    }
}
