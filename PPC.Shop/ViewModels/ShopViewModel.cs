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
using EasyMVVM;
using PPC.Data.Articles;
using PPC.Data.Contracts;
using PPC.Helpers;
using PPC.Messages;
using PPC.Popups;

namespace PPC.Shop.ViewModels
{
    public enum ShopStates
    {
        ShoppingCarts,
        Detail,
        SoldArticles
    }

    public class ShopViewModel : ObservableObject
    {
        private IPopupService PopupService => EasyIoc.IocContainer.Default.Resolve<IPopupService>();

        public const string ShopFilename = "_shop.xml";

        #region Shop state

        private ShopStates _shopState;
        public ShopStates ShopState
        {
            get { return _shopState; }
            set { Set(() => ShopState, ref _shopState, value); }
        }

        private ICommand _viewShoppingCartsCommand;
        public ICommand ViewShoppingCartsCommand => _viewShoppingCartsCommand = _viewShoppingCartsCommand ?? new RelayCommand(ViewShoppingCarts);

        private void ViewShoppingCarts()
        {
            SelectedButton = null;
            ShopState = ShopStates.ShoppingCarts;
        }

        private ICommand _viewSoldArticlesCommand;
        public ICommand ViewSoldArticlesCommand => _viewSoldArticlesCommand = _viewSoldArticlesCommand ?? new RelayCommand(ViewSoldArticle);

        private void ViewSoldArticle()
        {
            SelectedButton = null;
            ShopState = ShopStates.SoldArticles;
        }

        #endregion

        #region Cash register

        private CashRegisterViewModel _cashRegisterViewModel;
        public CashRegisterViewModel CashRegisterViewModel
        {
            get { return _cashRegisterViewModel; }
            set { Set(() => CashRegisterViewModel, ref _cashRegisterViewModel, value); }
        }

        #endregion

        #region Cash register/client shopping carts buttons

        private ShoppingCartBasedViewModelBase _selectedButton;
        public ShoppingCartBasedViewModelBase SelectedButton
        {
            get { return _selectedButton;}
            set
            {
                if (Set(() => SelectedButton, ref _selectedButton, value))
                {
                    if (value == null)
                        ShopState = ShopStates.ShoppingCarts; // switch back to summary when no button are selected
                    else
                        ShopState = ShopStates.Detail;
                    if (value == CashRegisterViewModel)
                        CashRegisterViewModel.ShoppingCart.IsArticleNameFocused = true; // TODO: grrrrrrrrr f**king focus
                }
            }
        }

        private ObservableCollection<ShoppingCartBasedViewModelBase> _buttons;
        public ObservableCollection<ShoppingCartBasedViewModelBase> Buttons
        {
            get { return _buttons; }
            protected set { Set(() => Buttons, ref _buttons, value); }
        }

        private ICommand _selectButtonCommand;
        public ICommand SelectButtonCommand => _selectButtonCommand = _selectButtonCommand ?? new RelayCommand<ShoppingCartBasedViewModelBase>(button => SelectedButton = button);

        private ICommand _addNewClientCommand;
        public ICommand AddNewClientCommand => _addNewClientCommand = _addNewClientCommand ?? new RelayCommand(AddNewClient);

        private void AddNewClient()
        {
            AskNamePopupViewModel vm = new AskNamePopupViewModel(PopupService, AddNewClientNameSelected);
            PopupService.DisplayModal(vm, "Client name?");
        }

        private void AddNewClientNameSelected(string name)
        {
            if (Buttons.OfType<ClientViewModel>().Any(x => x.ClientName == name))
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, "A shopping cart with that client name has already been opened!");
                PopupService.DisplayModal(vm, "Error");
            }
            else
            {
                ClientViewModel newClient = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    HasFullPlayerInfos = false,
                    ClientName = name,
                };
                Buttons.Add(newClient);
                SelectedButton = newClient;
                //
                RaisePropertyChanged(() => ClientShoppingCartsCount);
                RaisePropertyChanged(() => PaidClientShoppingCartsCount);
                RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
            }
        }

        private ICommand _closeClientCommand;
        public ICommand CloseClientCommand => _closeClientCommand = _closeClientCommand ?? new RelayCommand<ClientViewModel>(CloseClient);

        private void CloseClient(ClientViewModel client)
        {
            if (client.PaymentState == PaymentStates.Unpaid && client.ShoppingCart.ShoppingCartArticles.Any())
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", $"Client {client.ClientName} has not paid yet. shopping cart cannot be closed.",
                    new ActionButton
                    {
                        Caption = "Ok",
                        Order = 2,
                        ClickCallback = () => { }
                    });
            else
                PopupService.DisplayQuestion($"Close {client.ClientName} shopping cart", "Are you sure ?",
                    new ActionButton
                    {
                        Caption = "Yes",
                        Order = 1,
                        ClickCallback = () => CloseClientConfirmed(client)
                    },
                    new ActionButton
                    {
                        Caption = "No",
                        Order = 2,
                        ClickCallback = () => { }
                    });
        }

        private void CloseClientConfirmed(ClientViewModel client)
        {
            if (SelectedButton == client)
                SelectedButton = CashRegisterViewModel;
            Buttons.Remove(client);
            if (client.ShoppingCart.ShoppingCartArticles.Any())
            {
                // Create a transaction and add it to transactions
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
                AddTransaction(transaction);
            }
            // Delete backup file
            try
            {
                string filename = client.Filename;
                File.Delete(filename);
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Error while deleting backup file");
            }
            //
            RaisePropertyChanged(() => ClientShoppingCartsCount);
            RaisePropertyChanged(() => PaidClientShoppingCartsCount);
            RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
        }

        public int ClientShoppingCartsCount => Buttons.OfType<ClientViewModel>().Count();

        public int PaidClientShoppingCartsCount => Buttons.OfType<ClientViewModel>().Count(x => x.PaymentState == PaymentStates.Paid);

        public int UnpaidClientShoppingCartsCount => Buttons.OfType<ClientViewModel>().Count(x => x.PaymentState == PaymentStates.Unpaid);

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
                    SelectedButton = CashRegisterViewModel; // View cash register
                    // Reload transactions
                    LoadTransactions();
                    // Reload clients
                    //  remove existing clients
                    Buttons.RemoveOfType<ShoppingCartBasedViewModelBase, ClientViewModel>();
                    LoadClients();
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
                RefreshSoldArticles();
            }
            else
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, "Backup path not found");
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        private void LoadTransactions()
        {
            try
            {
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFilename}";
                if (File.Exists(filename))
                {
                    Data.Contracts.Shop shop;
                    using (XmlTextReader reader = new XmlTextReader(filename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(Data.Contracts.Shop));
                        shop = (Data.Contracts.Shop)serializer.ReadObject(reader);
                    }
                    Transactions.Clear();
                    Transactions.AddRange(shop.Transactions.Select(t => new ShopTransactionItem
                    {
                        Timestamp = t.Timestamp,
                        Articles = t.Articles.Select(a => new ShopArticleItem
                        {
                            Article = ArticlesDb.Instance.Articles.FirstOrDefault(x => x.Guid == a.Guid),
                            Quantity = a.Quantity
                        }).ToList(),
                        Cash = t.Cash,
                        BankCard = t.BankCard
                    }));
                    CashRegisterViewModel.Cash = Transactions.Sum(x => x.Cash);
                    CashRegisterViewModel.BankCard = Transactions.Sum(x => x.BankCard);
                }
                else
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, "Shop file not found");
                    PopupService.DisplayModal(vm, "Error while loading shop");
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Error while loading shop");
            }
        }

        private void LoadClients()
        {
            //  add backup clients
            foreach (string filename in Directory.EnumerateFiles(ConfigurationManager.AppSettings["BackupPath"], "*.xml", SearchOption.TopDirectoryOnly).Where(x => !x.Contains(ShopFilename)))
            {
                try
                {
                    ClientViewModel client = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles, filename);
                    Buttons.Add(client);
                }
                catch (Exception ex)
                {
                    ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                    PopupService.DisplayModal(vm, $"Error while loading {filename} cart");
                }
            }
        }

        #endregion

        #region Cash register closure

        private ICommand _cashRegisterClosureCommand;
        public ICommand CashRegisterClosureCommand => _cashRegisterClosureCommand = _cashRegisterClosureCommand ?? new RelayCommand(CashRegisterClosure);

        private void CashRegisterClosure()
        {
            if (Buttons.OfType<ClientViewModel>().Any(x => x.PaymentState == PaymentStates.Unpaid))
                PopupService.DisplayQuestion("Warning", "There is 1 or more client shopping cart opened.",
                    new ActionButton
                    {
                        Order = 1,
                        Caption = "Ok"
                    });
            else
            {
                PopupService.DisplayQuestion("Cash register closure", "Are you sure you want to perform closure ?",
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
            CashRegisterClosure closure = BuildClosure();

            // Display popup
            CashRegisterClosurePopupViewModel vm = new CashRegisterClosurePopupViewModel(PopupService, closure, CloseApplicationAfterClosure, async c => await SendCashRegisterClosureMailAsync(closure));
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
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(PopupService, ex);
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
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vmError, "Error");
            }
        }

        private CashRegisterClosure BuildClosure()
        {
            // Compute shop closure
            //  transactions from cash register
            List<TransactionFullArticle> transactions = new List<TransactionFullArticle>(Transactions.Select(t => new TransactionFullArticle
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
            //  transactions from shopping client carts
            foreach (ClientViewModel client in Buttons.OfType<ClientViewModel>())
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
            return closure;
        }

        private void CloseApplicationAfterClosure()
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
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vmError, "Error");
            }

            Application.Current.Shutdown();
        }

        private async Task SendCashRegisterClosureMailAsync(CashRegisterClosure closure)
        {
            // Send shop closure mail
            Mediator.Default.Send(new ChangeWaitingMessage { IsWaiting = true });
            try
            {
                string closureConfigFilename = ConfigurationManager.AppSettings["CashRegisterClosureConfigPath"];
                if (!File.Exists(closureConfigFilename))
                {
                    ErrorPopupViewModel vmError = new ErrorPopupViewModel(PopupService, "Cash register closure config file not found -> Cannot send automatically cash register closure mail.");
                    PopupService.DisplayModal(vmError, "Warning");
                }
                else
                {
                    // Read closure config
                    CashRegisterClosureConfig closureConfig;
                    using (XmlTextReader reader = new XmlTextReader(closureConfigFilename))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(CashRegisterClosureConfig));
                        closureConfig = (CashRegisterClosureConfig)await serializer.ReadObjectAsync(reader);
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
                ErrorPopupViewModel vmError = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vmError, "Error");
            }
            finally
            {
                Mediator.Default.Send(new ChangeWaitingMessage { IsWaiting = false });
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

        #region Transactions

        public ObservableCollection<ShopTransactionItem> Transactions { get; }

        private void AddTransaction(ShopTransactionItem transaction)
        {
            Transactions.Add(transaction);
            SaveTransactions();
            RefreshSoldArticles();
        }

        private void SaveTransactions()
        {
            try
            {
                if (!Directory.Exists(ConfigurationManager.AppSettings["BackupPath"]))
                    Directory.CreateDirectory(ConfigurationManager.AppSettings["BackupPath"]);
                Data.Contracts.Shop shop = new Data.Contracts.Shop
                {
                    Transactions = Transactions.Select(t => new ShopTransaction
                    {
                        Timestamp = t.Timestamp,
                        Articles = t.Articles.Select(a => new Item
                        {
                            Guid = a.Article.Guid,
                            Quantity = a.Quantity,
                        }).ToList(),
                        Cash = t.Cash,
                        BankCard = t.BankCard
                    }).ToList(),
                };
                string filename = $"{ConfigurationManager.AppSettings["BackupPath"]}{ShopFilename}";
                using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    DataContractSerializer serializer = new DataContractSerializer(typeof(Data.Contracts.Shop));
                    serializer.WriteObject(writer, shop);
                }
            }
            catch (Exception ex)
            {
                ErrorPopupViewModel vm = new ErrorPopupViewModel(PopupService, ex);
                PopupService.DisplayModal(vm, "Error while saving shop");
            }
        }

        #endregion

        public ShopViewModel()
        {
            ShopState = ShopStates.ShoppingCarts;

            Transactions = new ObservableCollection<ShopTransactionItem>();
            CashRegisterViewModel = new CashRegisterViewModel(AddTransaction);

            Buttons = new ObservableCollection<ShoppingCartBasedViewModelBase>
            {
                CashRegisterViewModel
            };

            Mediator.Default.Register<PlayerSelectedMessage>(this, PlayerSelected);
        }

        private void RefreshSoldArticles()
        {
            decimal totalCash = 0;
            decimal totalBankCard = 0;
            Dictionary<Guid, ShopArticleItem> soldItems = new Dictionary<Guid, ShopArticleItem>();
            // Gather sold items in closed transactions
            foreach (ShopArticleItem item in Transactions.SelectMany(x => x.Articles))
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
            totalCash += CashRegisterViewModel.Cash;
            totalBankCard += CashRegisterViewModel.BankCard;
            // Gather sold items in closed client shopping carts
            foreach (ClientViewModel client in Buttons.OfType<ClientViewModel>().Where(x => x.PaymentState == PaymentStates.Paid))
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
            //
            RaisePropertyChanged(() => PaidClientShoppingCartsCount);
            RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
        }

        private void PlayerSelected(PlayerSelectedMessage msg)
        {
            // Select shopping cart or create it
            ClientViewModel client = Buttons.OfType<ClientViewModel>().FirstOrDefault(x => x.DciNumber == msg.DciNumber && x.ClientFirstName == msg.FirstName && x.ClientLastName == msg.LastName);
            if (client == null)
            {
                ClientViewModel newClient = new ClientViewModel(RefreshSoldArticles, RefreshSoldArticles)
                {
                    HasFullPlayerInfos = true,
                    ClientName = msg.FirstName,
                    ClientFirstName = msg.FirstName,
                    ClientLastName = msg.LastName,
                    DciNumber = msg.DciNumber
                };
                Buttons.Add(newClient);
                SelectedButton = newClient;
                //
                RaisePropertyChanged(() => ClientShoppingCartsCount);
                RaisePropertyChanged(() => PaidClientShoppingCartsCount);
                RaisePropertyChanged(() => UnpaidClientShoppingCartsCount);
            }
            else
                SelectedButton = client;
        }
    }

    public class ShopViewModelDesignData : ShopViewModel
    {
        public ShopViewModelDesignData()
        {
            CashRegisterViewModel = new CashRegisterViewModelDesignData();

            Buttons = new ObservableCollection<ShoppingCartBasedViewModelBase>
            {
                CashRegisterViewModel,
                new ClientViewModelDesignData
                {
                    ClientName = "un super long nom0",
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
            Buttons.AddRange(Enumerable.Range(1, 5).Select(x => new ClientViewModelDesignData
            {
                ClientName = $"un super long nom[{x}]",
                PaymentState = x%2 == 0 ? PaymentStates.Unpaid : PaymentStates.Paid
            }));

            ShopState = ShopStates.ShoppingCarts;
        }
    }
}
