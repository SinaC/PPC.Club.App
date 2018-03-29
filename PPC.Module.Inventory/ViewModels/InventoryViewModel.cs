using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using EasyIoc;
using EasyMVVM;
using PPC.Common;
using PPC.Domain;
using PPC.IDataAccess;
using PPC.Log;
using PPC.Popups;
using PPC.Services.IO;
using PPC.Services.Popup;

namespace PPC.Module.Inventory.ViewModels
{
    public class InventoryViewModel : ObservableObject
    {
        private static readonly string[] EmptyList = { string.Empty };
        private IPopupService PopupService => IocContainer.Default.Resolve<IPopupService>();
        private ILog Logger => IocContainer.Default.Resolve<ILog>();
        private IArticleDL ArticlesDb => IocContainer.Default.Resolve<IArticleDL>();

        public IEnumerable<Article> Articles => ArticlesDb.Articles;
        public IEnumerable<string> Categories => EmptyList.Concat(ArticlesDb.Categories);
        public IEnumerable<string> Producers => EmptyList.Concat(ArticlesDb.Producers);
        private Func<string, IEnumerable<string>> BuildSubCategories => category => EmptyList.Concat(ArticlesDb.SubCategories(category));

        #region Selected article

        private Article _selectedArticle;
        public Article SelectedArticle
        {
            get { return _selectedArticle; }
            set { Set(() => SelectedArticle, ref _selectedArticle, value); }
        }

        #endregion

        #region Load

        private ICommand _loadCommand;
        public ICommand LoadCommand => _loadCommand = _loadCommand ?? new RelayCommand(Load);

        private void Load()
        {
            try
            {
                throw new NotImplementedException("Load is not anymore available");//ArticlesDb.Load();
                RaisePropertyChanged(() => Articles);
                PopupService.DisplayQuestion("Load", "Articles successfully loaded.", QuestionActionButton.Ok());
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while loading articles DB", ex);
                PopupService.DisplayError("Error while loading articles DB", ex);
            }
        }

        #endregion

        #region Save

        private ICommand _saveCommand;
        public ICommand SaveCommand => _saveCommand = _saveCommand ?? new RelayCommand(Save);

        private void Save()
        {
            try
            {
                Logger.Info("Saving articles DB.");
                throw new NotImplementedException("Save is not anymore available");//ArticlesDb.Save();
                PopupService.DisplayQuestion("Save", "Articles successfully saved.", QuestionActionButton.Ok());
                Logger.Info("Articles DB saved.");
            }
            catch (Exception ex)
            {
                Logger.Exception("Error while saving articles DB", ex);
                PopupService.DisplayError("Error while saving articles DB", ex);
            }
        }

        #endregion

        #region Import

        private ICommand _importCommand;
        public ICommand ImportCommand => _importCommand = _importCommand ?? new RelayCommand(Import);

        private void Import()
        {
            string path = System.IO.Path.GetDirectoryName(PPCConfigurationManager.ArticlesPath);

            IIOService ioService = new IOService();
            string filename = ioService.OpenFileDialog(path, ".dbf", "DBase VII documents (.dbf)|*.dbf");
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    Logger.Info("Importing articles DB.");
                    throw new NotImplementedException("Import from dbf is not anymore available");//ArticlesDb.ImportFromDbf(filename);
                    RaisePropertyChanged(() => Articles);
                    PopupService.DisplayQuestion("Import", $"{Articles.Count()} articles successfully imported. Don't forget to click on 'Save' button to save imported articles.", QuestionActionButton.Ok());
                    //!! after import -> every article guid are modified -> shopping carts and backup files are invalid
                    // TODO: 
                    //  import button must be disabled once a shopping cart/transaction has been created
                    //  backup files must be deleted once import is performed
                    Logger.Info("Articles DB imported.");
                }
                catch (Exception ex)
                {
                    Logger.Exception("Error while importing articles", ex);
                    PopupService.DisplayError("Error while importing articles", ex);
                }
            }
        }

        #endregion

        #region Article creation

        private ICommand _createArticleCommand;
        public ICommand CreateArticleCommand => _createArticleCommand = _createArticleCommand ?? new RelayCommand(DisplayCreateArticlePopup);

        private void DisplayCreateArticlePopup()
        {
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

            RaisePropertyChanged(() => Articles);

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

            RaisePropertyChanged(() => Articles);
            RaisePropertyChanged(() => Categories);

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
        }

        #endregion
    }

    public class InventoryViewModelDesignData : InventoryViewModel
    {
        public InventoryViewModelDesignData()
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
        }
    }
}
