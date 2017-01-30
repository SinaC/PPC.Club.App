using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.MVVM;
using PPC.Popup;
using PPC.Tab;

namespace PPC.Sale
{
    public class SaleViewModel : TabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region ShoppingCartTabBase

        public override string Header => "Sale";

        #endregion

        #region Tab management
        private ObservableCollection<ShoppingCartTabBase> _tabs;
        public ObservableCollection<ShoppingCartTabBase> Tabs
        {
            get { return _tabs; }
            set { Set(() => Tabs, ref _tabs, value); }
        }

        private ShoppingCartTabBase _selectedTab;
        public ShoppingCartTabBase SelectedTab
        {
            get { return _selectedTab; }
            set { Set(() => SelectedTab, ref _selectedTab, value); }
        }

        private ICommand _addNewTabCommand;
        public ICommand AddNewTabCommand => _addNewTabCommand = _addNewTabCommand ?? new RelayCommand(AddNewTab);
        private void AddNewTab()
        {
            AskNamePopupViewModel vm = new AskNamePopupViewModel(PopupService, AddNewTabNameSelected);
            PopupService.DisplayModal(vm, "Client name?");
        }

        private void AddNewTabNameSelected(string name)
        {
            ShoppingCartTabBase newTab = new ClientViewModel(ReshreshSoldArticles, ReshreshSoldArticles)
            {
                ClientName = name
            };
            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        #endregion

        #region Shop

        private ShopViewModel _shop;
        public ShopViewModel Shop
        {
            get { return _shop; }
            set { Set(() => Shop, ref _shop, value); }
        }

        #endregion

        #region Sold articles

        private List<SoldArticleItem> _soldArticles;
        public List<SoldArticleItem> SoldArticles
        {
            get {  return _soldArticles; }
            private set { Set(() => SoldArticles, ref _soldArticles, value); }
        }

        #endregion

        #region Reload

        private ICommand _reloadCommand;
        public ICommand ReloadCommand => _reloadCommand = _reloadCommand ?? new RelayCommand(Reload);
        private void Reload()
        {
            PopupService.DisplayQuestion("Reload", "Are you sure you want to reload from backup ?",
                   new ActionButton
                   {
                       Order = 1,
                       Caption = "Yes",
                       ClickCallback = ReloadConfirmed
                   },
                   new ActionButton
                   {
                       Order = 2,
                       Caption = "No"
                   });
        }

        private void ReloadConfirmed()
        {
            if (Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
            {
                try
                {
                    SelectedTab = Shop; // Select shop tab
                    // Reload shop (sold articles)
                    Shop.Load();
                    // Reload clients
                    //  remove existing clients
                    Tabs.RemoveOfType<ShoppingCartTabBase, ClientViewModel>();
                    //  add backup clients
                    foreach (string filename in Directory.GetFiles(ConfigurationManager.AppSettings["BackupPath"], "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(ShopViewModel.ShopFile)))
                    {
                        ClientCart cart;
                        using (XmlTextReader reader = new XmlTextReader(filename))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(ClientCart));
                            cart = (ClientCart) serializer.ReadObject(reader);
                        }
                        ClientViewModel client = new ClientViewModel(ReshreshSoldArticles, ReshreshSoldArticles)
                        {
                            ClientName = cart.Name,
                            IsCash = cart.IsCash,
                            IsPaid = cart.IsPaid,
                            ShoppingCart =
                            {
                                ShoppingCartArticles = new ObservableCollection<ShoppingCartArticleItem>(cart.Articles.Select(x => new ShoppingCartArticleItem
                                {
                                    Article = FakeArticleDb.Articles.FirstOrDefault(a => a.Ean == x.Ean), // TODO: search DB
                                    Quantity = x.Quantity
                                }))
                            },
                        };
                        Tabs.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
                ReshreshSoldArticles();
            }
            else
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel("Backup path not found");
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        #endregion

        #region Closing

        private ICommand _closingCommand;
        public ICommand ClosingCommand => _closingCommand = _closingCommand ?? new RelayCommand(Closing);
        private void Closing()
        {
            if (Tabs.OfType<ClientViewModel>().Any(x => !x.IsPaid))
                PopupService.DisplayQuestion("Warning", "There is 1 or more client shopping cart opened.",
                     new ActionButton
                     {
                         Order = 1,
                         Caption = "Ok"
                     });
            else
            {
                PopupService.DisplayQuestion("Closing", "Are you sure you want to close ?",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Yes",
                        ClickCallback = ClosingConfirmed
                    },
                    new ActionButton
                    {
                        Order = 2,
                        Caption = "No"
                    });
            }
        }

        private void ClosingConfirmed()
        {
            // TODO: 
            // compute sold articles and write them to file
            // remove backupfiles
        }

        #endregion

        public int SoldArticlesCount => SoldArticles?.Sum(x => x.QuantityBankCard + x.QuantityCash) ?? 0;
        public double SoldArticlesTotal => SoldArticles?.Sum(x => x.Total) ?? 0;
        public double SoldArticlesTotalBankCard => SoldArticles?.Sum(x => x.TotalBankCard) ?? 0;
        public double SoldArticlesTotalCash => SoldArticles?.Sum(x => x.TotalCash) ?? 0;

        public SaleViewModel()
        {
            Shop = new ShopViewModel(ReshreshSoldArticles);
            Tabs = new ObservableCollection<ShoppingCartTabBase>
            {
                Shop
            };
            SelectedTab = Shop;
        }

        private void ReshreshSoldArticles()
        {
            Dictionary<string, SoldArticleItem> soldItems = new Dictionary<string, SoldArticleItem>();
            // Gather sold items in current shopping cart
            foreach (ShopArticleItem item in Shop.SoldArticles)
            {
                SoldArticleItem soldItem;
                if (!soldItems.TryGetValue(item.Article.Ean, out soldItem))
                {
                    soldItem = new SoldArticleItem
                    {
                        Article = item.Article,
                        QuantityCash = 0,
                        QuantityBankCard = 0
                    };
                    soldItems.Add(item.Article.Ean, soldItem);
                }
                if (item.IsCash)
                    soldItem.QuantityCash += item.Quantity;
                else
                    soldItem.QuantityBankCard += item.Quantity;
            }
            // Gather sold items in closed client shopping carts
            foreach (ClientViewModel client in Tabs.OfType<ClientViewModel>().Where(x => x.IsPaid))
            {
                foreach (ShoppingCartArticleItem item in client.ShoppingCart.ShoppingCartArticles)
                {
                    SoldArticleItem soldItem;
                    if (!soldItems.TryGetValue(item.Article.Ean, out soldItem))
                    {
                        soldItem = new SoldArticleItem
                        {
                            Article = item.Article,
                            QuantityCash = 0,
                            QuantityBankCard = 0
                        };
                        soldItems.Add(item.Article.Ean, soldItem);
                    }
                    if (client.IsCash)
                        soldItem.QuantityCash += item.Quantity;
                    else
                        soldItem.QuantityBankCard += item.Quantity;
                }
            }
            SoldArticles = soldItems.Values.ToList();
            RaisePropertyChanged(() => SoldArticlesCount);
            RaisePropertyChanged(() => SoldArticlesTotal);
            RaisePropertyChanged(() => SoldArticlesTotalBankCard);
            RaisePropertyChanged(() => SoldArticlesTotalCash);
        }
    }

    public class SaleViewModelDesignData : SaleViewModel
    {
        public SaleViewModelDesignData()
        {
            Shop = new ShopViewModelDesignData();
            Tabs = new ObservableCollection<ShoppingCartTabBase>
            {
                Shop, new ClientViewModelDesignData
                {
                    ClientName = "Joel",
                    IsPaid = true
                }
            };
            SelectedTab = Tabs[0];
        }
    }
}
