using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.DataContracts;

namespace PPC.Shop.ViewModels
{
    public enum ArticleSelectorModes
    {
        CategorySelection,
        SubCategorySelection,
        ArticleSelection
    }

    public class ArticleSelectorViewModel : ObservableObject
    {
        public IEnumerable<string> Categories => ArticlesDb.Instance.Articles.Select(x => x.Category).Distinct();

        public IEnumerable<string> SubCategories => ArticlesDb.Instance.Articles.Where(x => x.Category == SelectedCategory && !String.IsNullOrWhiteSpace(x.SubCategory)).Select(x => x.SubCategory).Distinct();

        public IEnumerable<string> Producers => ArticlesDb.Instance.Articles.Select(x => x.Producer).Distinct();

        public IEnumerable<Article> Articles
        {
            get
            {
                IQueryable<Article> query = ArticlesDb.Instance.Articles.AsQueryable();
                if (!string.IsNullOrWhiteSpace(SelectedCategory))
                {
                    query = query.Where(x => x.Category == SelectedCategory);
                    if (!string.IsNullOrWhiteSpace(SelectedSubCategory))
                        query = query.Where(x => x.SubCategory == SelectedSubCategory);
                }
                return query.OrderBy(x => x.Description);
            }
        }

        private ICommand _backCommand;
        public ICommand BackCommand => _backCommand = _backCommand ?? new RelayCommand(Back, () => Mode == ArticleSelectorModes.ArticleSelection || Mode == ArticleSelectorModes.SubCategorySelection);

        private void Back()
        {
            switch (Mode)
            {
                case ArticleSelectorModes.ArticleSelection:
                    // if no subcategories, back to category selection
                    SelectedArticle = null;
                    if (SubCategories.Any())
                    {
                        SelectedSubCategory = null;
                        Mode = ArticleSelectorModes.SubCategorySelection;
                    }
                    else
                    {
                        SelectedCategory = null;
                        Mode = ArticleSelectorModes.CategorySelection;
                    }
                    break;
                case ArticleSelectorModes.SubCategorySelection:
                    SelectedCategory = null;
                    Mode = ArticleSelectorModes.CategorySelection;
                    break;
                default:
                    throw new InvalidOperationException($"Cannot use back button in mode {Mode}");
            }
        }

        private string _selectedCategory;

        public string SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (Set(() => SelectedCategory, ref _selectedCategory, value))
                {
                    SelectedSubCategory = null;
                    RaisePropertyChanged(() => SubCategories);
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
                Mode = ArticleSelectorModes.SubCategorySelection;
            else
            {
                Mode = ArticleSelectorModes.ArticleSelection;
                RaisePropertyChanged(() => Articles);
            }
        }

        private string _selectedSubCategory;

        public string SelectedSubCategory
        {
            get { return _selectedSubCategory; }
            set
            {
                if (Set(() => SelectedSubCategory, ref _selectedSubCategory, value))
                    RaisePropertyChanged(() => Articles);
            }
        }

        private ICommand _selectSubCategoryCommand;
        public ICommand SelectSubCategoryCommand => _selectSubCategoryCommand = _selectSubCategoryCommand ?? new RelayCommand<string>(SelectSubCategory);

        private void SelectSubCategory(string subCategory)
        {
            Mode = ArticleSelectorModes.ArticleSelection;
            SelectedSubCategory = subCategory;
        }

        private Article _selectedArticle;

        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set { Set(() => SelectedArticle, ref _selectedArticle, value); }
        }

        private ICommand _selectArticleCommand;
        public ICommand SelectArticleCommand => _selectArticleCommand = _selectArticleCommand ?? new RelayCommand<Article>(SelectArticle);

        private void SelectArticle(Article article)
        {
            SelectedArticle = article;
            Quantity = 1;
        }

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

        private ICommand _addArticleCommand;
        public ICommand AddArticleCommand => _addArticleCommand = _addArticleCommand ?? new RelayCommand(AddArticle, () => SelectedArticle != null);

        private void AddArticle()
        {
            // TODO: inform about article selection and quantity
            SelectedSubCategory = null;
            SelectedCategory = null;
            Mode = ArticleSelectorModes.CategorySelection;
        }

        private ArticleSelectorModes _mode;

        public ArticleSelectorModes Mode
        {
            get { return _mode; }
            set { Set(() => Mode, ref _mode, value); }
        }

        public ArticleSelectorViewModel()
        {
            Mode = ArticleSelectorModes.CategorySelection;
        }

        //public void Test()
        //{
        //    int noCategoryCount = ArticlesDb.Instance.Articles.Count(x => string.IsNullOrWhiteSpace(x.Category));
        //    int noSubCategoryCount = ArticlesDb.Instance.Articles.Count(x => string.IsNullOrWhiteSpace(x.SubCategory));

        //    foreach (string category in Categories)
        //    {
        //        SelectedCategory = category;
        //        foreach (string subCategory in SubCategories)
        //        {
        //            SelectedSubCategory = subCategory;
        //            int articleCount = Articles.Count();
        //            Debug.WriteLine($"{category}|{subCategory}:{articleCount}");
        //        }
        //    }
        //}
    }

    public class ArticleSelectorViewModelDesignData : ArticleSelectorViewModel
    {
        public ArticleSelectorViewModelDesignData()
        {
            ArticlesDb.Instance.Inject(new List<Article>
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
            });
            RaisePropertyChanged(() => Categories);
            RaisePropertyChanged(() => SubCategories);
            RaisePropertyChanged(() => Articles);

            SelectCategoryCommand.Execute("Cat1");
            SelectSubCategoryCommand.Execute("SubCategory11");
        }
    }
}
