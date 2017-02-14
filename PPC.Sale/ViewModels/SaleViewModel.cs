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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using PPC.DataContracts;
using PPC.Helpers;
using PPC.Messages;
using PPC.MVVM;
using PPC.Popup;

namespace PPC.Sale.ViewModels
{
    public class SaleViewModel : TabBase
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        #region TabBase

        public override string Header => "Sale";

        #endregion

        #region Tab management

        private ObservableCollection<ShoppingCartTabViewModelBase> _tabs;

        public ObservableCollection<ShoppingCartTabViewModelBase> Tabs
        {
            get { return _tabs; }
            protected set { Set(() => Tabs, ref _tabs, value); }
        }

        private ICommand _selectTabCommand;
        public ICommand SelectTabCommand => _selectTabCommand = _selectTabCommand ?? new RelayCommand<ShoppingCartTabViewModelBase>(tab => SelectedTab = tab);

        private ShoppingCartTabViewModelBase _selectedTab;

        public ShoppingCartTabViewModelBase SelectedTab
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
            if (Tabs.OfType<ClientViewModel>().Any(x => x.ClientName == name))
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel("A tab with that client name has already been opened!");
                PopupService.DisplayModal(vm, "Error");
            }
            else
            {
                ShoppingCartTabViewModelBase newTab = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    ClientName = name
                };
                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
        }

        private ICommand _closeTabCommand;
        public ICommand CloseTabCommand => _closeTabCommand = _closeTabCommand ?? new RelayCommand<ShoppingCartTabViewModelBase>(CloseTab);

        private void CloseTab(ShoppingCartTabViewModelBase tab)
        {
            ClientViewModel client = tab as ClientViewModel;
            if (client == null)
                return;
            if (client.PaymentState == PaymentStates.Unpaid && client.ShoppingCart.ShoppingCartArticles.Any())
                PopupService.DisplayQuestion($"Close {client.ClientName} tab", $"Client {client.ClientName} has not paid yet. Tab cannot be closed.",
                    new ActionButton
                    {
                        Caption = "Ok",
                        Order = 2,
                        ClickCallback = () => { }
                    });
            else
                PopupService.DisplayQuestion($"Close {client.ClientName} tab", "Are you sure ?",
                    new ActionButton
                    {
                        Caption = "Yes",
                        Order = 1,
                        ClickCallback = () => CloseTabConfirmed(client)
                    },
                    new ActionButton
                    {
                        Caption = "No",
                        Order = 2,
                        ClickCallback = () => { }
                    });
        }

        private void CloseTabConfirmed(ClientViewModel client)
        {
            if (SelectedTab == client)
                SelectedTab = Shop;
            Tabs.Remove(client);
            if (client.ShoppingCart.ShoppingCartArticles.Any())
            {
                // Create a transaction and add it to shop transactions
                ShopTransactionItem transaction = new ShopTransactionItem
                {
                    Timestamp = DateTime.Now,
                    Articles = client.ShoppingCart.ShoppingCartArticles.Select(x => new ShopArticleItem
                    {
                        Article = x.Article,
                        Quantity = x.Quantity
                    }).ToList(),
                    Cash = client.Cash,
                    BankCard = client.BankCard
                };
                Shop.AddTransactionFromClosedTab(transaction);
            }
            // Delete backup file
            try
            {
                string filename = client.Filename;
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vm, "Error while deleting backup file");
            }
            //
            RefreshSoldArticles();
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
            get { return _soldArticles; }
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
                    Tabs.RemoveOfType<ShoppingCartTabViewModelBase, ClientViewModel>();
                    //  add backup clients
                    foreach (string filename in Directory.EnumerateFiles(ConfigurationManager.AppSettings["BackupPath"], "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(ShopViewModel.ShopFile)))
                    {
                        try
                        {
                            ClientViewModel client = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles, filename);
                            Tabs.Add(client);
                        }
                        catch (Exception ex)
                        {
                            ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                            PopupService.DisplayModal(vm, $"Error while loading {filename} cart");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(ex);
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
                RefreshSoldArticles();
            }
            else
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel("Backup path not found");
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        #endregion

        #region Cash register closure

        private ICommand _cashRegisterClosureCommand;
        public ICommand CashRegisterClosureCommand => _cashRegisterClosureCommand = _cashRegisterClosureCommand ?? new RelayCommand(CashRegisterClosure);

        private void CashRegisterClosure()
        {
            if (Tabs.OfType<ClientViewModel>().Any(x => x.PaymentState == PaymentStates.Unpaid))
                PopupService.DisplayQuestion("Warning", "There is 1 or more client shopping cart opened.",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Ok"
                    });
            else
            {
                PopupService.DisplayQuestion("Cash register closure", "Are you sure you want to close ?",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Yes",
                        ClickCallback = async () => await CashRegisterClosureConfirmedAsync()
                    },
                    new ActionButton
                    {
                        Order = 2,
                        Caption = "No"
                    });
            }
        }

        private async Task CashRegisterClosureConfirmedAsync()
        {
            // Compute cash register closure
            //  transactions from shop
            List<TransactionFullArticle> transactions = new List<TransactionFullArticle>(Shop.Transactions.Select(t => new TransactionFullArticle
            {
                Timestamp = t.Timestamp,
                Articles = t.Articles.Select(x => new FullArticle
                {
                    Guid = x.Article.Guid,
                    Ean = x.Article.Ean,
                    Description = x.Article.Description,
                    Price = x.Article.Price,
                    Quantity = x.Quantity
                }).ToList(),
                Cash = t.Cash,
                BankCard = t.BankCard
            }));
            //  transactions from tabs
            foreach (ClientViewModel client in Tabs.OfType<ClientViewModel>())
            {
                transactions.Add(new TransactionFullArticle
                {
                    Timestamp = client.PaymentTimestamp,
                    Articles = client.ShoppingCart.ShoppingCartArticles.Select(x => new FullArticle
                    {
                        Guid = x.Article.Guid,
                        Ean = x.Article.Ean,
                        Description = x.Article.Description,
                        Price = x.Article.Price,
                        Quantity = x.Quantity
                    }).ToList(),
                    Cash = client.Cash,
                    BankCard = client.BankCard
                });
            }
            //  closure summary
            CashRegisterClosure closure = new CashRegisterClosure
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
                BankCard = SoldArticlesTotalBankCard,
                Transactions = transactions
            };

            // Display popup
            CashRegisterClosurePopupViewModel vm = new CashRegisterClosurePopupViewModel(PopupService, closure, CloseApplication, async c => await SendCashRegisterClosureMailAsync(closure));
            PopupService.DisplayModal(vm, "Cash register closure"); // !! Shutdown application on close

            // Dump cash register closure file
            DateTime now = DateTime.Now;
            //  txt
            try
            {
                string filename = $"{ConfigurationManager.AppSettings["CashRegisterClosurePath"]}CashRegister_{now:yyyy-MM-dd_HH-mm-ss}.txt";
                File.WriteAllText(filename, closure.ToString());
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }
            //  xml
            try
            {
                string filename = $"{ConfigurationManager.AppSettings["CashRegisterClosurePath"]}CashRegister_{now:yyyy-MM-dd_HH-mm-ss}.xml";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterClosure));
                    await serializer.WriteObjectAsync(writer, closure);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }
        }

        private void CloseApplication()
        {
            // Delete backup files
            try
            {
                string backupPath = ConfigurationManager.AppSettings["BackupPath"];
                foreach (string file in Directory.EnumerateFiles(backupPath))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }

            Application.Current.Shutdown();
        }

        private async Task SendCashRegisterClosureMailAsync(CashRegisterClosure closure)
        {
            // Send cash register closure mail
            Mediator.Default.Send(new ChangeWaitingMessage {IsWaiting = true});
            try
            {
                string closureConfigFilename = ConfigurationManager.AppSettings["CashRegisterClosureConfigPath"];
                if (!File.Exists(closureConfigFilename))
                {
                    ErrorPopupViewModel vmError = new ErrorPopupViewModel("Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
                    PopupService.DisplayModal(vmError, "Warning");
                }
                else
                {
                    CashRegisterClosureConfig closureConfig;
                    using (XmlTextReader reader = new XmlTextReader(closureConfigFilename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterClosureConfig));
                        closureConfig = (CashRegisterClosureConfig) await serializer.ReadObjectAsync(reader);
                    }

                    // Send mail
                    MailAddress fromAddress = new MailAddress(closureConfig.SenderMail, "From PPC Club");
                    MailAddress toAddress = new MailAddress(closureConfig.RecipientMail, "To PPC");
                    using (SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, closureConfig.SenderPassword)
                    })
                    {

                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = $"Cloture caisse du club (date {DateTime.Now:F})",
                            Body = closure.ToString()
                        })
                        {
                            await client.SendMailAsync(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(ex);
                PopupService.DisplayModal(vmError, "Error");
            }
            finally
            {
                Mediator.Default.Send(new ChangeWaitingMessage {IsWaiting = false});
            }
        }

        #endregion

        #region Computed values

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

        #endregion

        public SaleViewModel()
        {
            if (!DesignMode.IsInDesignModeStatic)
            {
                ArticleDb.Load();
                //ArticleDb.ImportFromCsv();
            }
            //ArticleDb.Import();
            //ArticleDb.Save();

            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);

            //
            Shop = new ShopViewModel(RefreshSoldArticles);
            Tabs = new ObservableCollection<ShoppingCartTabViewModelBase>
            {
                Shop
            };
            SelectedTab = Shop;
        }

        private void RefreshSoldArticles()
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
            foreach (ClientViewModel client in Tabs.OfType<ClientViewModel>().Where(x => x.PaymentState == PaymentStates.Paid))
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
            //
            SoldArticles = soldItems.Values.ToList();
            // Compute count/total/cash/bc
            SoldArticlesCount = SoldArticles.Sum(x => x.Quantity);
            SoldArticlesTotal = SoldArticles.Sum(x => x.Total);
            SoldArticlesTotalCash = totalCash;
            SoldArticlesTotalBankCard = totalBankCard;
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            // Select tab or create it
            ClientViewModel client = Tabs.OfType<ClientViewModel>().FirstOrDefault(x => (x.DciNumber == msg.DciNumber && x.ClientFirstName == msg.FirstName && x.ClientLastName == msg.LastName));
            if (client == null)
            {
                ShoppingCartTabViewModelBase newTab = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    ClientName = msg.FirstName,
                    ClientFirstName = msg.FirstName,
                    ClientLastName = msg.LastName,
                    DciNumber = msg.DciNumber
                };
                Tabs.Add(newTab);
                SelectedTab = newTab;
            }
            else
                SelectedTab = client;
        }
    }

    public class SaleViewModelDesignData : SaleViewModel
    {
        public SaleViewModelDesignData()
        {
            Shop = new ShopViewModelDesignData();
            Tabs = new ObservableCollection<ShoppingCartTabViewModelBase>
            {
                Shop,
                new ClientViewModelDesignData
                {
                    ClientName = $"un super long nom0",
                    PaymentState = PaymentStates.Unpaid
                },

                new ClientViewModelDesignData
                {
                    ClientName = "Joel",
                    PaymentState = PaymentStates.Paid
                },
                new ClientViewModelDesignData
                {
                    ClientName = "Pouet",
                    PaymentState = PaymentStates.Unpaid
                }
            };
            Tabs.AddRange(Enumerable.Range(1, 5).Select(x => new ClientViewModelDesignData
            {
                ClientName = $"un super long nom[{x}]",
                PaymentState = x%2 == 0 ? PaymentStates.Unpaid : PaymentStates.Paid
            }));
            SelectedTab = Tabs[0];
        }
    }
}
