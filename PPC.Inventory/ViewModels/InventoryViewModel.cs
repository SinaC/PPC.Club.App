using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Popups;
using PPC.Services;

namespace PPC.Inventory.ViewModels
{
    public class InventoryViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        private static readonly string[] EmptyCategory = { string.Empty };

        public IEnumerable<Article> Articles => ArticlesDb.Instance.Articles;

        private IEnumerable<string> Categories => EmptyCategory.Concat(ArticlesDb.Instance.Articles.GroupBy(x => x.Category, (category, articles) => category));
        private IEnumerable<string> Producers => ArticlesDb.Instance.Articles.GroupBy(x => x.Producer, (producer, articles) => producer);

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
                ArticlesDb.Instance.Load();
                RaisePropertyChanged(() => Articles);
                PopupService.DisplayQuestion("Load", "Articles successfully loaded.", new ActionButton
                {
                    Caption = "Ok",
                    Order = 1
                });
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Load error");
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
                ArticlesDb.Instance.Save();
                PopupService.DisplayQuestion("Save", "Articles successfully saved.", new ActionButton
                {
                    Caption = "Ok",
                    Order = 1
                });
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Save error");
            }
        }

        #endregion

        #region Import

        private ICommand _importCommand;
        public ICommand ImportCommand => _importCommand = _importCommand ?? new RelayCommand(Import);

        private void Import()
        {
            string path = System.IO.Path.GetDirectoryName(ConfigurationManager.AppSettings["ArticlesPath"]);

            IIOService ioService = new IOService();
            string filename = ioService.OpenFileDialog(path, ".dbf", "DBase VII documents (.dbf)|*.dbf");
            if (!string.IsNullOrWhiteSpace(filename))
            {
                try
                {
                    ArticlesDb.Instance.ImportFromDbf(filename);
                    RaisePropertyChanged(() => Articles);
                    PopupService.DisplayQuestion("Import", $"{Articles.Count()} articles successfully imported. Don't forget to click on 'Save' button to save imported articles.",
                        new ActionButton
                        {
                            Caption = "Ok",
                            Order = 1
                        });
                    //!! after import -> every article guid are modified -> shopping carts and backup files are invalid
                    // TODO: 
                    //  import button must be disabled once a shopping cart has been created
                    //  backup files must be deleted once import is performed
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                    PopupService.DisplayModal(vm, "Import error");
                }
            }
        }

        #endregion

        #region Article creation

        private ICommand _createArticleCommand;
        public ICommand CreateArticleCommand => _createArticleCommand = _createArticleCommand ?? new RelayCommand(DisplayCreateArticlePopup);

        private void DisplayCreateArticlePopup()
        {
            CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(PopupService, Categories, Producers, CreateArticle)
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
                ArticlesDb.Instance.Add(article);
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel errorVm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(errorVm, "Error while saving articles DB");
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
                CreateEditArticlePopupViewModel vm = new CreateEditArticlePopupViewModel(PopupService, Categories, Producers, SaveArticle)
                {
                    IsEdition = true,
                    Ean = SelectedArticle.Ean,
                    Description = SelectedArticle.Description,
                    Category = SelectedArticle.Category,
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
                ErrorPopupViewModel errorVm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(errorVm, "Error while saving articles DB");
            }
        }

        #endregion
    }

    public class InventoryViewModelDesignData : InventoryViewModel
    {
        public InventoryViewModelDesignData()
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
        }
    }
}
