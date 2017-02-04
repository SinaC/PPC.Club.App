using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.MVVM;
using PPC.Popup;

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
            protected set { Set(() => Tabs, ref _tabs, value); }
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

        private List<ShopArticleItem> _soldArticles;
        public List<ShopArticleItem> SoldArticles
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
                        ClientViewModel client = new ClientViewModel(ReshreshSoldArticles, ReshreshSoldArticles);
                        client.Load(filename);
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
            // Compute closing
            Closing closing = new Closing
            {
                Articles = SoldArticles?.Select(x => new FullArticle
                {
                    Guid = x.Article.Guid,
                    Ean = x.Article.Ean,
                    Description = x.Article.Description,
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = SoldArticlesTotalCash,
                BankCard = SoldArticlesTotalBankCard
            };

            // Display popup
            ClosingPopupViewModel vm = new ClosingPopupViewModel(PopupService, () => Application.Current.Shutdown(), closing);
            PopupService.DisplayModal(vm, "Closing"); // !! Shutdown application on close

            try
            {
                // Dump closing file
                string filename = ConfigurationManager.AppSettings["ClosingPath"];
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Closing));
                    serializer.WriteObject(writer, closing);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }
            try
            {
                string closingConfigFilename = ConfigurationManager.AppSettings["ClosingConfigPath"];
                if (!File.Exists(closingConfigFilename))
                {
                    ErrorPopupViewModel vmError = new ErrorPopupViewModel("Closing config file not found -> Cannot send automatically closing mail.");
                    PopupService.DisplayModal(vmError, "Error");
                }
                else
                {
                    ClosingConfig closingConfig;
                    using (XmlTextReader reader = new XmlTextReader(closingConfigFilename))
                    {
                        
                        DataContractSerializer serializer = new DataContractSerializer(typeof(ClosingConfig));
                        closingConfig = (ClosingConfig) serializer.ReadObject(reader);
                    }

                    // Send mail
                    MailAddress fromAddress = new MailAddress(closingConfig.SenderMail, "From PPC Club");
                    MailAddress toAddress = new MailAddress(closingConfig.RecipientMail, "To PPC");
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, closingConfig.SenderPassword)
                    };
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Cloture de la caisse du club (date: {DateTime.Now:F})");
                    sb.AppendLine($"Cash: {closing.Cash:C}");
                    sb.AppendLine($"Bancontact: {closing.BankCard:C}");
                    sb.AppendLine("Articles:");
                    if (SoldArticles != null)
                    {
                        foreach (FullArticle article in closing.Articles)
                            sb.AppendLine($"{article.Quantity,5} * {article.Ean,15} {article.Description,30} {article.Price,10:C}");
                    }
                    else
                        sb.AppendLine("néant");
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = $"Cloture caisse du club (date {DateTime.Now:F})",
                        Body = sb.ToString()
                    })
                    {
                        client.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }
        }

        #endregion

        private int _soldArticlesCount;
        public int SoldArticlesCount
        {
            get { return _soldArticlesCount; }
            set { Set(() => SoldArticlesCount, ref _soldArticlesCount, value); }
        }

        private decimal _soldArticlesTotal;
        public decimal SoldArticlesTotal
        {
            get { return _soldArticlesTotal; }
            set { Set(() => SoldArticlesTotal, ref _soldArticlesTotal, value); }
        }

        private decimal _soldArticlesTotalCash;
        public decimal SoldArticlesTotalCash
        {
            get { return _soldArticlesTotalCash; }
            set { Set(() => SoldArticlesTotalCash, ref _soldArticlesTotalCash, value); }
        }

        private decimal _soldArticlesTotalBankCard;
        public decimal SoldArticlesTotalBankCard
        {
            get { return _soldArticlesTotalBankCard; }
            set { Set(() => SoldArticlesTotalBankCard, ref _soldArticlesTotalBankCard, value); }
        }

        public SaleViewModel()
        {
            ArticleDb.Load();
            //ArticleDb.Import();
            //ArticleDb.Save();

            //
            Shop = new ShopViewModel(ReshreshSoldArticles);
            Tabs = new ObservableCollection<ShoppingCartTabBase>
            {
                Shop
            };
            SelectedTab = Shop;
        }

        private void ReshreshSoldArticles()
        {
            decimal totalCash = 0;
            decimal totalBankCard = 0;
            Dictionary<Guid, ShopArticleItem> soldItems = new Dictionary<Guid, ShopArticleItem>();
            // Gather sold items in current shopping cart
            foreach (ShopArticleItem item in Shop.Transactions.SelectMany(x => x.Articles))
            {
                ShopArticleItem soldItem;
                if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
                {
                    soldItem = new ShopArticleItem
                    {
                        Article = item.Article,
                        Quantity = 0
                    };
                    soldItems.Add(item.Article.Guid, soldItem);
                }
                soldItem.Quantity += item.Quantity;
            }
            totalCash += Shop.Cash;
            totalBankCard += Shop.BankCard;
            // Gather sold items in closed client shopping carts
            foreach (ClientViewModel client in Tabs.OfType<ClientViewModel>().Where(x => x.IsPaid))
            {
                foreach (ShopArticleItem item in client.ShoppingCart.ShoppingCartArticles)
                {
                    ShopArticleItem soldItem;
                    if (!soldItems.TryGetValue(item.Article.Guid, out soldItem))
                    {
                        soldItem = new ShopArticleItem
                        {
                            Article = item.Article,
                            Quantity = 0,
                        };
                        soldItems.Add(item.Article.Guid, soldItem);
                    }
                    soldItem.Quantity += item.Quantity;
                }
                totalCash += client.Cash;
                totalBankCard += client.BankCard;
            }
            SoldArticles = soldItems.Values.ToList();

            SoldArticlesCount = SoldArticles.Sum(x => x.Quantity);
            SoldArticlesTotal = SoldArticles.Sum(x => x.Total);
            SoldArticlesTotalCash = totalCash;
            SoldArticlesTotalBankCard = totalBankCard;
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
